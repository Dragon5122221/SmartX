using SmartXWebApi.Services;
using SmartX.Shared.Realtime;
using SmartX.Shared.Sensors;
using SmartX.Shared.Telemetry;
using SmartX.Shared.Contracts.Telemetry;
internal static class Part2Checks
{
    public static async Task Run(Action<bool,string> check)
    {
        var g=new GatewayService();g.Stop();var q=new QueueStatus(0,0,0,0,0,0);
        StreamSnapshot Read(string id="SX-0003",string? category=null,string? health=null)=>g.Stream(q,null,category,health,id);
        var a=g.Command(new("SX-0003",true));var b=g.Command(new("SX-0003",false));
        check(Read().SelectedOverride==false&&Read().UndoDepth==2,"P2: commands update state and push stack");
        bool conflict=false;try{g.Undo(a.Id);}catch(GatewayException ex){conflict=ex.Status==409;}
        check(conflict&&Read().UndoDepth==2,"P2: stale undo token cannot pop another command");
        g.Undo(b.Id);check(Read().SelectedOverride==true,"P2: undo restores manual state");
        g.Undo(a.Id);check(Read().SelectedOverride==null&&Read().UndoDepth==0,"P2: undo restores automatic mode");
        bool bad=false;try{g.Command(new("SX-0001",true));}catch(GatewayException){bad=true;}
        check(bad,"P2: read-only sensors reject overrides");
        g.Register(new(){Id="CHECK-POWER",Name="Priority test meter",Category=SensorCategory.PowerConsumption,LocationId="zone-a",IntervalSeconds=60});
        var now=DateTimeOffset.UtcNow;
        IngestionResult Power(long n,int value)=>g.IngestInteger(new(){Batches=[[new(){SensorId="CHECK-POWER",Sequence=n,Timestamp=now.AddMilliseconds(n),Value=value}]]});
        Power(1,2000);Power(2,2100);check(Read().ActiveErrors==1&&Read().DuplicateAlertsSuppressed==1,"P2: HashSet suppresses repeated anomalies");
        Power(3,400);check(Read().ActiveErrors==0,"P2: recovery clears error key");
        Power(4,2200);check(Read().ActiveErrors==1,"P2: recurrence after recovery becomes active");
        check(Read("CHECK-POWER").Timeline.Select(x=>x.Sequence).SequenceEqual(new long[]{1,2,3,4}),"P2: SortedDictionary retains chronological readings");
        g.RecordSearch(new("SX-0003"));g.RecordSearch(new("Irrigation valve 0003"));
        check(Read().Suggestions.Any(s=>s.Kind=="Search pattern"&&s.Observations==2),"P2: contextual search evidence produces suggestions");
        for(int i=0;i<3;i++){g.Command(new("SX-0003",true));g.Command(new("SX-0003",false));}
        check(Read().Suggestions.Any(s=>s.Kind=="Learned action"&&s.Enabled==true&&s.Observations==3),"P2: repeated contextual commands produce learned suggestion");
        Power(5,400);check(!Read().Suggestions.Any(s=>s.Kind=="Learned action"),"P2: clearing trigger removes learned suggestion");
        g.Register(new(){Id="CHECK-TREND",Name="Forecast test",Category=SensorCategory.Environmental,LocationId="zone-a",IntervalSeconds=60});
        g.IngestFloat(new(){Batches=[Enumerable.Range(0,6).Select(i=>new TelemetryPacket<float>{SensorId="CHECK-TREND",Sequence=i,Timestamp=DateTimeOffset.UtcNow.AddSeconds(-5+i),Value=27+i*.6f}).ToArray()]});
        check(Read("CHECK-TREND").Suggestions.Any(s=>s.Kind=="Trend forecast"&&s.SensorId=="CHECK-TREND"),"P2: regression predicts threshold crossing");
        check(Read(category:"Actuator").Devices.All(d=>d.Category=="Actuator"),"P2: category filter enforced");
        check(Read(health:"Anomalous").Devices.All(d=>d.Health=="Anomalous"),"P2: alert-state filter enforced");
        using var dispatch=new TelemetryDispatcher();var order=new List<int>();
        IngestionResult Apply(TelemetryBatchRequest<int> request){order.Add(request.Batches[0][0].Value);return new(1,0,0,[]);}
        TelemetryBatchRequest<int> Batch(params int[] values)=>new(){Batches=[values.Select((v,i)=>new TelemetryPacket<int>{SensorId="TEST",Sequence=i,Timestamp=DateTimeOffset.UtcNow,Value=v}).ToArray()]};
        var normal=dispatch.Submit(Batch(1,2,3),v=>v>100,Apply,CancellationToken.None);
        var urgent=dispatch.Submit(Batch(999,998),v=>v>100,Apply,CancellationToken.None);
        check(dispatch.Status().StandardDepth==3&&dispatch.Status().CriticalDepth==2,"P2: packets enter real FIFO and priority queues");
        await dispatch.StartAsync(CancellationToken.None);await Task.WhenAll(normal,urgent).WaitAsync(TimeSpan.FromSeconds(5));
        check(order.SequenceEqual(new[]{999,998,1,2,3}),"P2: critical packets bypass FIFO; equal priorities preserve arrival order");
        check(dispatch.Status().CriticalProcessed==2&&dispatch.Status().StandardProcessed==3,"P2: counters reflect processing");
        await dispatch.StopAsync(CancellationToken.None);
        using var full=new TelemetryDispatcher();var big=Batch(Enumerable.Repeat(1,4096).ToArray());big=big with{Batches=[big.Batches[0],big.Batches[0]]};
        var p1=full.Submit(big,_=>false,Apply,CancellationToken.None);var p2=full.Submit(big,_=>false,Apply,CancellationToken.None);
        bool capacity=false;try{await full.Submit(Batch(1),_=>false,Apply,CancellationToken.None);}catch(GatewayException ex){capacity=ex.Status==429;}
        check(capacity,"P2: capacity guard returns 429");await full.StartAsync(CancellationToken.None);await Task.WhenAll(p1,p2).WaitAsync(TimeSpan.FromSeconds(10));await full.StopAsync(CancellationToken.None);
    }
}
