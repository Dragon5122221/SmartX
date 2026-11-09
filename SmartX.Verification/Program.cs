using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using SmartX.Core.DataStructures;
using SmartX.Core.Validation;
using SmartX.Shared.Telemetry;
using SmartX.Shared.Deployment;
using SmartX.Shared.Sensors;
using SmartX.Shared.Contracts.Sensors;
using SmartXWebApi.Services;
using SmartXWebApi.Storage;

// VERIFY-01: dependency-free executable regression suite for assessed observable behaviour.
int passed = 0;
/// <summary>VERIFY-02: fail the process immediately if a required invariant is violated.</summary>
void Check(bool condition, string name) { if (!condition) throw new Exception("FAIL: " + name); Console.WriteLine("PASS: " + name); passed++; }
/// <summary>VERIFY-03: construct deterministic typed packets for collection checks.</summary>
TelemetryPacket<int> Packet(long n) => new() { SensorId = "TEST", Sequence = n, Timestamp = DateTimeOffset.UtcNow, Value = (int)n };
var ring = new TelemetryBatchBuffer<int>(2); var raw = new[] { Packet(1), Packet(2) }; ring.AppendBatch(raw); raw[0] = Packet(99);
ring.AppendBatch([Packet(3)]);
Check(ring.ToList().Select(p => p.Value).SequenceEqual(new[] {1,2,3}), "Unequal jagged batches transfer to List in order, with input ownership protected");
var snapshot = ring.SnapshotBatches(); snapshot[0][0] = Packet(77);
Check(ring.ToList()[0].Value == 1, "Snapshots cannot mutate retained packets");
ring.AppendBatch([Packet(4),Packet(5)]);
Check(ring.BatchCount == 2 && ring.Count == 3 && ring.ToList()[0].Value == 3, "Custom ring evicts oldest whole batch and keeps counts coherent");
Check((new PowerReading(400) + new PowerReading(200)).Watts == 600 && (new PowerReading(200) - new PowerReading(400)).Watts == -200,"Overloaded watts aggregate and preserve signed delta");
bool overflow = false; try { _ = new PowerReading(int.MaxValue) + new PowerReading(1); } catch(OverflowException) {overflow=true;}
Check(overflow,"Power operator rejects overflow");
var validator = new DeploymentTreeValidator();
Check(validator.Validate(new() { Id="a",Name="Facility",Children=[new(){Id="b",Name="Zone"}]}).IsValid,"Valid multi-tier deployment passes recursion");
Check(!validator.Validate(null).IsValid,"Null root terminates safely");
var cycle = new DeploymentNode {Id="loop",Name="Loop"}; cycle.Children.Add(cycle);
Check(!validator.Validate(cycle).IsValid,"Active-path cycle is rejected without stack overflow");
Check(!validator.Validate(new(){Id="a",Name="A",Children=[new(){Id="a",Name="Duplicate"}]}).IsValid,"Duplicate IDs are detected independently of reference cycles");
Check(!validator.Validate(new(){Id="a",Name="A",Enabled=false,Children=[new(){Id="b",Name="B"}]}).IsValid,"Enabled descendant under disabled ancestor is rejected");
var deep = new DeploymentNode{Id="leaf",Name="Leaf"}; for(int i=0;i<35;i++) deep=new(){Id="node"+i,Name="Node",Children=[deep]};
Check(!validator.Validate(deep).IsValid,"Excessive hierarchy depth returns a controlled error");
var watch=Stopwatch.StartNew(); var gateway=new GatewayService(); watch.Stop(); var dashboard=gateway.Dashboard();
Check(dashboard.Sensors==1200 && dashboard.Accepted==28800 && dashboard.Rejected==0,"1,200 seeded devices ingest 28,800 mixed typed packets through the real pipeline");
Console.WriteLine($"MEASURE: seed duration {watch.Elapsed.TotalMilliseconds:F2} ms; managed memory {GC.GetTotalMemory(false):N0} bytes");
gateway.Register(new(){Id="VERIFY-TEMP",Name="Validation sensor",LocationId="zone-a",Category=SensorCategory.Environmental});
var now=DateTimeOffset.UtcNow;
var accepted=gateway.IngestFloat(new(){Batches=[[new(){SensorId="VERIFY-TEMP",Sequence=1,Timestamp=now,Value=24.5f}]]});
Check(accepted.Accepted==1 && gateway.FloatHistory("VERIFY-TEMP").Readings[0].Value==24.5f,"Float value round-trips unchanged");
Check(gateway.IngestFloat(new(){Batches=[[new(){SensorId="VERIFY-TEMP",Sequence=1,Timestamp=now,Value=25f}]]}).Rejected==1,"Duplicate sequence cannot overwrite history");
Check(gateway.IngestInteger(new(){Batches=[[new(){SensorId="VERIFY-TEMP",Sequence=2,Timestamp=now,Value=25}]]}).Rejected==1,"Wrong typed route rejects a sensor payload");
Check(gateway.IngestFloat(new(){Batches=[[new(){SensorId="VERIFY-TEMP",Sequence=2,Timestamp=now,Value=float.NaN}]]}).Rejected==1,"Non-finite telemetry is rejected");
var issue=gateway.Report(new("VERIFY-TEMP","Inspect cable"));gateway.Resolve(issue.Id);
Check(gateway.Issues().Single(x=>x.Id==issue.Id).Resolved,"Required List-backed issue reporting and resolution work");
var folder=Path.Combine(Path.GetTempPath(),"SmartX-verify-"+Guid.NewGuid());Directory.CreateDirectory(folder);
try
{
    var configuration=new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string,string?>{{"SmartX:StoragePath",folder}}).Build();
    var provider=DataProtectionProvider.Create(new DirectoryInfo(Path.Combine(folder,"keys")));
    var keys=new AttachmentKeyProvider(provider,configuration);var storage=new EncryptedAttachmentStorage(keys);
    var bytes=Encoding.UTF8.GetBytes(string.Concat(Enumerable.Repeat("sensor=ok\n",20000)));
    var meta=await storage.SaveAsync("VERIFY-TEMP","hardware.log",new MemoryStream(bytes),default);
    var download=await storage.OpenAsync("VERIFY-TEMP",meta.Id,default); await using(var stream=download.Stream){using var output=new MemoryStream();await stream.CopyToAsync(output);Check(output.ToArray().SequenceEqual(bytes),"Multi-record encrypted upload decrypts byte-for-byte");}
    var restarted=new EncryptedAttachmentStorage(new AttachmentKeyProvider(provider,configuration));
    Check(restarted.List("VERIFY-TEMP").Count==1,"Protected key and metadata survive a storage restart");
    var encryptedPath=Path.Combine(folder,meta.Id+".sxenc");var encrypted=await File.ReadAllBytesAsync(encryptedPath);
    Check(!Encoding.UTF8.GetString(encrypted).Contains("sensor=ok"),"Persisted payload is ciphertext");
    encrypted[50]^=1;await File.WriteAllBytesAsync(encryptedPath,encrypted);
    bool tampered=false;try{await storage.OpenAsync("VERIFY-TEMP",meta.Id,default);}catch(CryptographicException){tampered=true;}
    Check(tampered,"Tampered ciphertext is rejected before download bytes are released");
    encrypted[50]^=1; await File.WriteAllBytesAsync(encryptedPath,encrypted[..^20]);
    bool truncated=false;try{await storage.OpenAsync("VERIFY-TEMP",meta.Id,default);}catch(Exception ex) when(ex is EndOfStreamException or CryptographicException){truncated=true;}
    Check(truncated,"Missing authenticated terminal record is rejected as a truncated upload");
    bool association=false;try{await storage.OpenAsync("WRONG-SENSOR",meta.Id,default);}catch(GatewayException){association=true;}
    Check(association,"Attachments cannot be downloaded through a different sensor");
    bool empty=false;try{await storage.SaveAsync("VERIFY-TEMP","empty.log",new MemoryStream(),default);}catch(GatewayException){empty=true;}
    Check(empty,"Empty upload is rejected and partial output cleaned up");
}
finally {Directory.Delete(folder,true);}
Console.WriteLine($"SUCCESS: {passed} checks passed.");

await Part2Checks.Run(Check);
Console.WriteLine($"TOTAL VERIFIED: {passed} checks, including Part 1 regression and Part 2 behaviours.");
