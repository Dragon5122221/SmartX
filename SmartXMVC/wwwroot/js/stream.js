/* Command desk: bounded snapshots, explicit operator intent and evidence-linked suggestions. */
let streamState = {selected:'SX-0001',page:1,paused:false}, streamSnapshot, streamSelected, pendingCommand, streamBusy=false, streamReload=false;
async function loadStream() {
    if(page!=='stream') return;
    if(streamBusy) {streamReload=true;return;}
    streamBusy=true;const selectedId=streamState.selected;
    try {
        const query=new URLSearchParams({search:$('#stream-search').value.trim(),category:$('#stream-category').value,health:$('#stream-health').value,selected:streamState.selected,page:streamState.page});
        const d=await api('stream?'+query);
        const selected=await api('sensors/'+encodeURIComponent(selectedId));
        if(selectedId!==streamState.selected){streamReload=true;return;}
        streamSnapshot=d;streamSelected=selected;
        const stat=(name,value,detail)=>`<div class="ribbon-stat"><span class="eyebrow">${esc(name)}</span><strong>${esc(value)}</strong><small>${esc(detail)}</small></div>`;
        $('#stream-metrics').innerHTML=stat('LIVE REGISTRY',d.totalDevices.toLocaleString(),'Unique device identifiers')+stat('STANDARD / CRITICAL',`${d.queue.standardDepth} / ${d.queue.criticalDepth}`,`${d.queue.standardProcessed.toLocaleString()} FIFO · ${d.queue.criticalProcessed.toLocaleString()} priority processed`)+stat('ACTIVE ERROR STATES',d.activeErrors,`${d.duplicateAlertsSuppressed.toLocaleString()} repeat anomalies suppressed`)+stat('LAST QUEUE LATENCY',`${d.queue.lastLatencyMs.toFixed(1)} ms`,'Enqueue to processing completion');
        $('#stream-updated').textContent=(streamState.paused?'PAUSED · ':'UPDATED · ')+utc(d.timestamp)+' UTC';
        // Preserve keyboard focus when the live list is redrawn.
        const focused=document.activeElement?.dataset.device;
        $('#stream-devices').innerHTML=d.devices.length?d.devices.map(s=>`<button class="fleet-device" data-device="${esc(s.id)}" aria-pressed="${s.id===streamState.selected}"><span><strong>${esc(s.id)}</strong><span class="health-mark ${esc(s.health)}">${esc(s.health)}</span></span><small>${esc(s.name)}</small><small>${esc(s.zone)} / ${s.value===null?'—':esc(s.category==='Actuator'?(s.value?'OPEN':'CLOSED'):s.value.toFixed(s.category==='Environmental'?1:0))} ${esc(s.unit)}</small></button>`).join(''):'<p class="empty">No matching devices. Clear a filter or try another name.</p>';
        if(focused) document.querySelector(`[data-device="${CSS.escape(focused)}"]`)?.focus({preventScroll:true});
        $('#stream-page').textContent=`${streamState.page} / ${Math.max(1,Math.ceil(d.matchingDevices/12))} · ${d.matchingDevices} matches`;
        $('#stream-prev').disabled=streamState.page===1;$('#stream-next').disabled=streamState.page*12>=d.matchingDevices;
        const s=selected.profile, last=d.timeline.at(-1);
        $('#stream-device-name').textContent=s.name;$('#stream-device-meta').textContent=`${s.id} / ${s.locationId} / ${selected.health}`;
        $('#stream-value').innerHTML=`${esc(last?(s.category==='Actuator'?(last.value?'OPEN':'CLOSED'):last.value.toFixed(s.category==='Environmental'?1:0)):'—')} <small>${esc(s.unit)}</small>`;
        chart(d.timeline,s.unit,s.valueKind);
        $('#stream-range').textContent=`${d.timeline.length} samples · last ${last?utc(last.timestamp)+' UTC':'awaiting telemetry'}`;
        $('#stream-inspect').href='/Sensors/Details/'+encodeURIComponent(s.id);
        const active=d.selectedOverride===null?null:{enabled:d.selectedOverride};
        $('#override-state').textContent=s.category==='Actuator'?(active?`Requested: ${active.enabled?'OPEN':'CLOSED'} · manual mode`:'Automatic mode · no active override'):'Read-only sensor · select an actuator to control';
        $('#command-open').disabled=s.category!=='Actuator'||active?.enabled===true;
        $('#command-close').disabled=s.category!=='Actuator'||active?.enabled===false;
        const focusedInsight=document.activeElement?.dataset.insight;
        $('#stream-insights').innerHTML=d.suggestions.length?d.suggestions.map((a,i)=>`<article class="insight-item"><span class="insight-no">${String(i+1).padStart(2,'0')}</span><div><small>${esc(a.kind)}</small><h3>${esc(a.title)}</h3><p>${esc(a.evidence)}</p></div><button class="button compact" data-insight="${i}">${a.enabled===null?'Inspect ↗':'Review action ↗'}</button></article>`).join(''):'<p class="empty">No supported predictions yet. Repeated exact-device searches and manual commands during an active anomaly build evidence; six consistent samples support trend forecasts.</p>';
        if(focusedInsight!==undefined)document.querySelector(`[data-insight="${focusedInsight}"]`)?.focus({preventScroll:true});
        $('#stream-history').innerHTML=d.commands.length?d.commands.map(c=>`<tr><td class="mono">${esc(new Date(c.timestamp).toISOString().replace('T',' ').slice(0,19))}</td><td><button class="button compact" data-device="${esc(c.sensorId)}">${esc(c.sensorId)}</button></td><td>${c.enabled?'OPEN':'CLOSED'}</td><td>${c.previousOverride===null?'AUTOMATIC':c.previousOverride?'MANUAL OPEN':'MANUAL CLOSED'}</td><td>${c.undone?'UNDONE':'ISSUED'}</td></tr>`).join(''):'<tr><td colspan="5" class="empty">No manual commands yet. Select an actuator above to begin.</td></tr>';
        $('#command-undo').disabled=d.undoDepth===0;
    } finally {streamBusy=false;if(streamReload && page==='stream'){streamReload=false;queueMicrotask(()=>loadStream().catch(e=>notify(e.message,true)));}}
}
function reviewCommand(sensorId, enabled) {
    pendingCommand={sensorId,enabled};
    $('#command-confirm-title').textContent=`${enabled?'Open':'Close'} ${sensorId}?`;
    $('#command-confirm-detail').textContent='This sets a manual override on the simulated actuator until you undo it. The device reports its observed state on the next telemetry tick. Check the selected device and site conditions.';
    $('#command-dialog').showModal();
}
async function demoDevice(id,category,name) {
    // Explicit registration is idempotent under a race; conflicts are resolved by reading the existing profile.
    try{return await api('sensors/'+id);}catch(error){if(!error.message.includes('Sensor not found'))throw error;}
    try{await api('sensors','POST',{id,name,locationId:'zone-c',category,intervalSeconds:60});}
    catch(error){if(!error.message.includes('already exists'))throw error;}
    return api('sensors/'+id);
}
async function runExperiment(kind) {
    const trend=kind==='trend', id=trend?'DEMO-TREND':kind==='burst'?'DEMO-FIFO':'DEMO-POWER';
    const type=trend?'float':'integer';
    await demoDevice(id,trend?'Environmental':'PowerConsumption',trend?'Trend forecast experiment':kind==='burst'?'FIFO burst experiment':'Priority power experiment');
    const history=await api(`telemetry/${type}/${id}/history`), sequence=(history.readings.at(-1)?.sequence??-1)+1;
    const count=kind==='burst'?120:trend?6:1;
    const now=Date.now();
    if(trend && history.readings.length && now-new Date(history.readings.at(-1).timestamp).getTime()<6000) throw new Error("Wait six seconds before generating another rising trend.");
    const packets=Array.from({length:count},(_,i)=>({sensorId:id,sequence:sequence+i,timestamp:new Date(now-(count-1-i)*(trend?1000:1)).toISOString(),value:trend?27+i*.6:kind==='spike'?2200:420+i%15}));
    const result=await api('telemetry/'+type,'POST',{batches:[packets]});
    streamState.selected=id;
    $('#stream-lab-result').textContent=`${result.accepted} accepted · ${result.rejected} rejected · ${id}. ${result.errors.map(e=>e.message).join(' ')}`;
    await loadStream();
}
function bindStream() {
    streamBusy=false;
    $('#stream-pause').textContent=streamState.paused?'Resume view':'Pause view';
    on('#stream-filter','submit',async()=>{streamState.page=1;await api('stream/search','POST',{query:$('#stream-search').value.trim(),category:$('#stream-category').value,health:$('#stream-health').value});await loadStream();});
    on('#stream-clear','click',async()=>{$('#stream-search').value='';$('#stream-category').value='';$('#stream-health').value='';streamState.page=1;await loadStream();});
    on('#stream-prev','click',async()=>{streamState.page=Math.max(1,streamState.page-1);await loadStream();});
    on('#stream-next','click',async()=>{streamState.page++;await loadStream();});
    on('#stream-pause','click',async()=>{streamState.paused=!streamState.paused;$('#stream-pause').textContent=streamState.paused?'Resume view':'Pause view';if(!streamState.paused)await loadStream();else $('#stream-updated').textContent='PAUSED · simulation continues';});
    on('#command-open','click',()=>reviewCommand(streamState.selected,true));
    on('#command-close','click',()=>reviewCommand(streamState.selected,false));
    $('#command-dialog').addEventListener('close',async()=>{if($('#command-dialog').returnValue!=='confirm'||!pendingCommand)return;const command=pendingCommand;pendingCommand=null;try{await api('stream/commands','POST',command);notify('Override issued. Awaiting the next device sample.');await loadStream();}catch(e){notify(e.message,true);}});
    on('#command-undo','click',async()=>{const top=streamSnapshot?.undoId;if(!top)throw new Error('Refresh the command history before undoing.');await api('stream/undo/'+top,'POST',{});notify('Last command undone. Previous control mode restored.');await loadStream();});
    on('#stream-burst','click',()=>runExperiment('burst'));on('#stream-spike','click',()=>runExperiment('spike'));on('#stream-recover','click',()=>runExperiment('recover'));on('#stream-trend','click',()=>runExperiment('trend'));
    on('#stream-export','click',()=>{
        if(!streamSnapshot?.commands.length)throw new Error('Issue a command before exporting its history.');
        const cell=v=>'"'+String(v??'automatic').replace(/"/g,'""')+'"';
        const csv=['Id,UTC,Device,Enabled,PreviousOverride,Undone',...streamSnapshot.commands.map(c=>[c.id,c.timestamp,c.sensorId,c.enabled,c.previousOverride,c.undone].map(cell).join(','))].join('\r\n');
        const url=URL.createObjectURL(new Blob([csv],{type:'text/csv;charset=utf-8'}));const a=document.createElement('a');a.href=url;a.download='SmartX-command-history.csv';a.click();setTimeout(()=>URL.revokeObjectURL(url),1000);
    });
    $('#workspace').addEventListener('click',streamClick);
}
async function streamClick(e) {
    if(page!=='stream')return;
    const device=e.target.closest('[data-device]'), insight=e.target.closest('[data-insight]');
    try {
        if(device){streamState.selected=device.dataset.device;await loadStream();}
        if(insight){const action=streamSnapshot.suggestions[Number(insight.dataset.insight)];if(action.enabled!==null)reviewCommand(action.sensorId,action.enabled);else{streamState.selected=action.sensorId;await loadStream();}}
    }catch(error){notify(error.message,true);}
}
