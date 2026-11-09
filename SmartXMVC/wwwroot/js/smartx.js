/* UI-01: original Smart-X interaction layer. All domain data crosses the MVC/API boundary. */
'use strict';
const $ = selector => document.querySelector(selector);
let page = document.body.dataset.page, routeGeneration = 0, delegatedBound = false;
let noticeTimer, paused = false, sensorPage = 1, uploadController;
// DATA STRUCTURE UI-DS-01: small client state holds only the currently inspected snapshot, never the ingestion store.
let currentSensor, currentTree;
/** UI-02: HTML-escape all server/user strings before placing them in structured markup. */
function esc(value) { return String(value ?? '').replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c])); }
/** UI-03: show accessible success/error feedback without blocking the workspace. */
function notify(message, error = false) { if(message==='Navigation superseded') return; const box = $('#notice'); clearTimeout(noticeTimer); box.textContent = message; box.className = 'notice' + (error ? ' error' : ''); box.hidden = false; if(error && $('#connection').textContent === 'API unavailable') { const retry = document.createElement('button'); retry.className = 'button compact'; retry.textContent = 'Retry connection'; retry.addEventListener('click', () => location.reload()); box.append(document.createElement('br'), retry); } noticeTimer = setTimeout(() => box.hidden = true, error ? 12000 : 6000); }
/** UI-04: async same-origin request with anti-forgery header, timeout, and meaningful API validation errors. */
async function api(path, method = 'GET', data, signal) {
    const generation = routeGeneration;
    const headers = {'X-CSRF-TOKEN': $('meta[name="csrf-token"]').content};
    const options = {method, headers, signal: signal ?? AbortSignal.timeout(30000)};
    if (data !== undefined) { if (data instanceof FormData) options.body = data; else { headers['Content-Type'] = 'application/json'; options.body = JSON.stringify(data); } }
    let response;
    try { response = await fetch('/gateway/' + path, options); }
    catch (error) { if (error.name !== 'AbortError') connection(false); throw new Error(error.name === 'AbortError' ? 'Upload cancelled.' : 'The gateway request timed out or the connection was interrupted. Retry when the API is available.'); }
    const result = await response.json().catch(() => ({}));
    if(generation!==routeGeneration) throw new Error('Navigation superseded');
    if (!response.ok) {
        if (response.status >= 500) connection(false);
        throw new Error(result.detail || (result.errors ? Object.values(result.errors).flat().join(' ') : result.title) || `Request failed (${response.status}).`);
    }
    connection(true); return result;
}
/** UI-05: reflect real API availability in the header. */
function connection(online) { const item = $('#connection'); item.textContent = online ? 'API online' : 'API unavailable'; item.className = 'status ' + (online ? 'healthy' : 'disconnected'); }
/** UI-06: format timestamps explicitly in UTC, matching the telemetry table. */
function utc(time) { return new Date(time).toLocaleTimeString('en-GB', {timeZone:'UTC',hour12:false}); }
/** UI-07: render a non-colour-only device status badge. */
function badge(health) { return `<span class="status ${esc(health.toLowerCase())}">${esc(health)}</span>`; }
/** UI-08: create one labelled numerical measure; every displayed figure comes from the API. */
function metric(label, value, sub, colour = '') { return `<div class="metric"><span class="eyebrow">${esc(label)}</span><div class="metric-value ${colour}">${esc(value)}</div><div class="metric-sub">${esc(sub)}</div></div>`; }
/** UI-09: render an accessible original SVG trend with real timestamp spacing and precise point tooltips. */
function chart(points, unit = '', kind = 'Float') {
    const host = $('#chart'); if (!host) return;
    if (!points.length) { host.innerHTML = '<p class="empty">No readings yet. Send a packet from the ingestion lab.</p>'; return; }
    const width = 720, height = 220, left = 48, right = 18, top = 16, bottom = 33;
    let minimum = Math.min(...points.map(p => p.value)), maximum = Math.max(...points.map(p => p.value));
    if (kind === 'Boolean') { minimum = 0; maximum = 1; }
    else { const margin = Math.max((maximum - minimum) * .18, .5); minimum -= margin; maximum += margin; }
    const start = new Date(points[0].timestamp).getTime(), finish = new Date(points.at(-1).timestamp).getTime();
    const x = p => left + ((new Date(p.timestamp).getTime() - start) / Math.max(finish - start, 1)) * (width - left - right);
    const y = value => height - bottom - (value - minimum) / (maximum - minimum || 1) * (height - top - bottom);
    const path = points.map((p, i) => `${i ? 'L' : 'M'}${x(p).toFixed(2)},${y(p.value).toFixed(2)}`).join(' ');
    const grid = Array.from({length:4}, (_, i) => { const value = minimum + (maximum - minimum) * i / 3; return `<line x1="${left}" x2="${width-right}" y1="${y(value)}" y2="${y(value)}" stroke="#2a3830" stroke-dasharray="2 5"/><text x="${left-9}" y="${y(value)+4}" text-anchor="end">${value.toFixed(kind === 'Integer' ? 0 : 1)}</text>`; }).join('');
    const dots = points.map(p => { const abnormal = kind === 'Float' ? p.value > 32 || p.value < 10 : kind === 'Integer' ? p.value > 1500 : false; return `<circle cx="${x(p)}" cy="${y(p.value)}" r="${abnormal ? 4 : 2}" fill="${abnormal ? '#e8b46c' : '#a5d8b1'}"><title>${esc(utc(p.timestamp))} UTC · ${esc(p.value.toFixed(kind==='Float'?2:0))} ${esc(unit)}</title></circle>`; }).join('');
    host.innerHTML = `<svg viewBox="0 0 ${width} ${height}" role="img" aria-label="${esc(points.length)} readings from ${esc(utc(points[0].timestamp))} to ${esc(utc(points.at(-1).timestamp))} UTC, measured in ${esc(unit)}"><defs><linearGradient id="signal-fill" x1="0" y1="0" x2="0" y2="1"><stop stop-color="#a5d8b1" stop-opacity=".12"/><stop offset="1" stop-color="#a5d8b1" stop-opacity="0"/></linearGradient></defs>${grid}<path d="${path} L${x(points.at(-1))},${height-bottom} L${left},${height-bottom} Z" fill="url(#signal-fill)"/><path d="${path}" stroke="#a5d8b1" stroke-width="2" fill="none"/>${dots}<text x="${left}" y="${height-8}">${esc(utc(points[0].timestamp))}</text><text x="${width-right}" y="${height-8}" text-anchor="end">${esc(utc(points.at(-1).timestamp))} UTC</text></svg>`;
}
/** UI-10: update dashboard metrics, trend, actionable alerts and simulation state from one snapshot. */
async function dashboard() {
    const id = $('#trend-id').value.trim() || 'SX-0001';
    const d = await api('dashboard?sensorId=' + encodeURIComponent(id));
    $('#metrics').innerHTML = metric('REGISTERED DEVICES', d.sensors.toLocaleString(), `${d.healthy} reporting normally`, 'mint') + metric('NEEDS ATTENTION', d.anomalous + d.disconnected, `${d.anomalous} anomalous / ${d.disconnected} disconnected`, 'amber') + metric('PACKETS ACCEPTED', d.accepted.toLocaleString(), `${d.rejected} rejected by validation`) + metric('RETAINED READINGS', d.retained.toLocaleString(), `${d.lastBatchMilliseconds.toFixed(2)} ms · last typed batch`);
    const row = await api('sensors/' + encodeURIComponent(id));
    $('#trend-name').textContent = row.profile.id + ' / ' + row.profile.unit;
    $('#trend-current').textContent = row.displayValue + ' ' + row.profile.unit;
    $('#trend-link').href = '/Sensors/Details/' + encodeURIComponent(id);
    chart(d.trend, row.profile.unit, row.profile.valueKind);
    $('#watchlist').innerHTML = d.watchlist.map(s => `<a class="watch-row" href="/Sensors/Details/${encodeURIComponent(s.profile.id)}"><div><strong>${esc(s.profile.id)}</strong><small>${esc(s.profile.locationId)} · ${esc(s.displayValue)} ${esc(s.profile.unit)}</small></div>${badge(s.health)}</a>`).join('');
    $('#alerts').innerHTML = d.alerts.length ? d.alerts.map(a => `<div class="alert-row"><span class="alert-line ${a.kind === 'Disconnect' ? 'disconnect' : ''}"></span><div><strong>${esc(a.kind)} / ${esc(a.sensorId)}</strong><p>${esc(a.message)}</p><small>${esc(utc(a.timestamp))} UTC${a.acknowledged ? ' · ACKNOWLEDGED' : ''}</small></div><div class="alert-actions"><a href="/Sensors/Details/${encodeURIComponent(a.sensorId)}">Investigate ↗</a>${a.acknowledged ? '' : `<button class="button compact" data-ack="${esc(a.id)}">Acknowledge</button>`}</div></div>`).join('') : '<p class="empty">No alert events recorded. The signal is clear.</p>';
    $('#sim-state').textContent = `${d.simulation.running ? 'RUNNING' : 'STOPPED'} / ${d.simulation.scenario.toUpperCase()} / ${d.simulation.devices} DEVICES / TICK ${d.simulation.ticks}`;
    $('#chart-range').textContent = `${d.trend.length} samples · snapshot ${utc(d.timestamp)} UTC`;
}
/** UI-11: list/search/paginate the sensor registry, never downloading the whole device fleet into the browser. */
async function sensors() {
    const query = new URLSearchParams({search:$('#search').value, category:$('#category-filter').value, page:sensorPage});
    const rows = await api('sensors?' + query);
    $('#sensor-rows').innerHTML = rows.length ? rows.map(s => `<tr><td><strong>${esc(s.profile.id)}</strong><small>${esc(s.profile.name)}</small></td><td>${esc(s.profile.locationId)}</td><td>${esc(s.profile.category)}</td><td class="mono">${esc(s.displayValue)} ${esc(s.profile.unit)}</td><td>${badge(s.health)}</td><td><a href="/Sensors/Details/${encodeURIComponent(s.profile.id)}">Inspect ↗</a></td></tr>`).join('') : '<tr><td colspan="6" class="empty">No matching sensors. Try a different search.</td></tr>';
    $('#page-number').textContent = `PAGE ${String(sensorPage).padStart(2,'0')}`; $('#previous').disabled = sensorPage === 1; $('#next').disabled = rows.length < 30;
}
/** UI-12: traverse only the bounded, server-validated tree to populate location options. */
function locations(node, prefix = '') { return [{id:node.id, name:prefix+node.name, enabled:node.enabled}, ...node.children.flatMap(child => locations(child, prefix+'— '))]; }
/** UI-13: populate registration/edit controls; immutable type/identifier fields are locked during editing. */
async function sensorForm() {
    const tree = await api('deployment');
    $('#sensor-location').innerHTML = locations(tree).filter(x => x.enabled).map(x => `<option value="${esc(x.id)}">${esc(x.name)}</option>`).join('');
    if (page === 'sensor-edit') {
        const row = await api('sensors/' + encodeURIComponent(routeId())); const s = row.profile;
        $('#sensor-id').value = s.id; $('#sensor-id').readOnly = true; $('#sensor-name').value = s.name; $('#sensor-location').value = s.locationId; $('#sensor-category').value = s.category; $('#sensor-category').disabled = true; $('#sensor-interval').value = s.intervalSeconds;
    }
}
/** UI-14: extract the sensor identifier from the MVC details/edit/history route. */
function routeId() { const id = decodeURIComponent(location.pathname.split('/').filter(Boolean).at(-1) || ''); return ['Details','Edit','History'].includes(id) ? 'SX-0001' : id; }
/** UI-15: refresh sensor context, health, signal history and independent attachment metadata. */
async function details() {
    const id = routeId(); const s = await api('sensors/' + encodeURIComponent(id)); currentSensor = s.profile;
    $('#device-id').textContent = s.profile.id + ' / INVESTIGATION'; $('#device-name').textContent = s.profile.name; $('#device-location').textContent = `${s.profile.locationId} · ${s.profile.category} · ${s.profile.simulated ? 'Simulated device' : 'Manually registered device'}`;
    $('#edit-link').href = '/Sensors/Edit/' + encodeURIComponent(id); $('#history-link').href = '/Telemetry/History/' + encodeURIComponent(id);
    $('#device-metrics').innerHTML = metric('LATEST READING', s.displayValue, s.profile.unit,'mint') + metric('DEVICE HEALTH', s.health, 'Three missed intervals → disconnected') + metric('REPORTING CADENCE', s.profile.intervalSeconds + ' s', 'Expected interval') + metric('LAST RECEIVED', s.profile.lastSeen ? utc(s.profile.lastSeen) : '—', 'Server receipt time · UTC');
    $('#device-type').textContent = s.profile.valueKind.toUpperCase();
    const h = await api(`telemetry/${s.profile.valueKind.toLowerCase()}/${encodeURIComponent(id)}/history`);
    chart(h.readings.map(p => ({timestamp:p.timestamp,value:typeof p.value === 'boolean' ? +p.value : p.value})), s.profile.unit, s.profile.valueKind);
    await attachments();
}
/** UI-16: render safe download links; the backend enforces sensor association and authentication of ciphertext. */
async function attachments() {
    const id = routeId(), files = await api(`sensors/${encodeURIComponent(id)}/attachments`);
    $('#attachments').innerHTML = files.length ? files.map(f => `<div class="attachment-row"><div>${esc(f.fileName)}<small>${(f.length/1024).toFixed(1)} KiB · ${esc(utc(f.uploadedAt))} UTC · ENCRYPTED</small></div><a href="/gateway/sensors/${encodeURIComponent(id)}/attachments/${encodeURIComponent(f.id)}/download">Download ↓</a></div>`).join('') : '<p class="empty">No attachments yet.</p>';
}
/** UI-17: show explicit array-batch/list evidence alongside a chronological packet table. */
async function telemetryHistory() {
    const id = routeId(), row = await api('sensors/' + encodeURIComponent(id));
    const h = await api(`telemetry/${row.profile.valueKind.toLowerCase()}/${encodeURIComponent(id)}/history`);
    $('#profile-link').href = '/Sensors/Details/' + encodeURIComponent(id); $('#history-title').textContent = id + ' / ' + row.profile.valueKind;
    $('#batch-info').textContent = `${h.batchCount} RETAINED BATCHES → ${h.readings.length} VISIBLE READINGS`;
    chart(h.readings.map(p => ({timestamp:p.timestamp,value:typeof p.value === 'boolean' ? +p.value : p.value})),row.profile.unit,row.profile.valueKind);
    $('#history-rows').innerHTML = h.readings.length ? h.readings.slice().reverse().map(p => `<tr><td class="mono">${esc(p.sequence)}</td><td>${esc(p.sensorId)}</td><td class="mono">${esc(new Date(p.timestamp).toISOString())}</td><td class="mono">${esc(p.value)} ${esc(row.profile.unit)}</td></tr>`).join('') : '<tr><td colspan="4">No packets received yet.</td></tr>';
}
/** UI-18: render a hierarchy with native keyboard-accessible expandable details. */
function treeMarkup(node) { return `<details open><summary>${esc(node.name)}</summary><small>${esc(node.id)} / ${node.enabled ? 'ENABLED' : 'DISABLED'}</small>${node.children.map(treeMarkup).join('')}</details>`; }
/** UI-19: load the saved configuration into the tree inspector and editable JSON area. */
async function deployment() { currentTree = await api('deployment'); $('#tree').innerHTML = treeMarkup(currentTree); $('#tree-json').value = JSON.stringify(currentTree,null,2); }
/** UI-20: validate or save edited JSON and display all returned node paths. */
async function validateTree(save) {
    let root; try { root = JSON.parse($('#tree-json').value); } catch { throw new Error('Configuration is not valid JSON. Check commas, brackets and quoted property names.'); }
    const result = await api(save ? 'deployment' : 'deployment/validate', save ? 'PUT' : 'POST', save ? root : {root});
    $('#tree-result').textContent = result.isValid ? `VALID / ${result.nodesVisited} nodes visited recursively.${save ? '\nConfiguration saved.' : '\nNo changes saved.'}` : result.issues.map(x => `${x.path}: ${x.message}`).join('\n');
    if (result.isValid && save) { await deployment(); notify('Deployment configuration saved.'); }
}
/** UI-21: populate the issue journal with real report and resolution state. */
async function issues() {
    const rows = await api('dashboard/issues');
    $('#issue-list').innerHTML = rows.length ? rows.map(i => `<article class="journal-row"><div><a href="/Sensors/Details/${encodeURIComponent(i.sensorId)}">${esc(i.sensorId)} ↗</a><p>${esc(i.description)}</p><small>${esc(new Date(i.createdAt).toISOString())}</small></div><div>${i.resolved ? '<span class="status">Resolved</span>' : `<button class="button compact" data-resolve="${esc(i.id)}">Mark resolved</button>`}</div></article>`).join('') : '<div class="empty">No reported issues. Open a sensor profile to add an investigation note.</div>';
}
/** UI-22: wrap events so rejected async operations always become visible feedback. */
function on(selector, event, handler) { const element = $(selector); if (element) element.addEventListener(event, async e => { if (event === 'submit') e.preventDefault(); const button = e.submitter ?? (element.tagName === 'BUTTON' ? element : null); if (button) button.disabled = true; try { await handler(e); } catch (error) { notify(error.message,true); if ($('#form-error')) $('#form-error').textContent = error.message; } finally { if (button) button.disabled = false; } }); }
/** UI-23: bind feature-specific controls once, avoiding duplicate event handlers during refresh. */
function bind() {
    on('#trend-form','submit',dashboard);
    on('#pause-view','click',() => { paused = !paused; $('#pause-view').textContent = paused ? 'Resume view' : 'Pause view'; });
    on('#sim-start','click',async () => { await api('simulation/start','POST',{scenario:$('#scenario').value}); notify('Scenario running.'); await dashboard(); });
    on('#sim-stop','click',async () => { await api('simulation/stop','POST',{}); notify('Simulation stopped. Health monitoring remains active.'); await dashboard(); });
    on('#search-form','submit',async () => { sensorPage=1; await sensors(); });
    on('#previous','click',async () => { sensorPage=Math.max(1,sensorPage-1); await sensors(); });
    on('#next','click',async () => { sensorPage++; await sensors(); });
    on('#sensor-form','submit',async () => {
        const request={id:$('#sensor-id').value.trim(),name:$('#sensor-name').value.trim(),locationId:$('#sensor-location').value,category:$('#sensor-category').value,intervalSeconds:Number($('#sensor-interval').value)};
        const result=await api(page==='sensor-edit'?'sensors/'+encodeURIComponent(routeId()):'sensors',page==='sensor-edit'?'PUT':'POST',request);
        location.href='/Sensors/Details/'+encodeURIComponent(result.id);
    });
    on('#ingest-form','submit',async () => {
        const kind=$('#ingest-kind').value, raw=$('#ingest-value').value.trim(); let value;
        if(kind==='boolean') { if(!['true','false'].includes(raw.toLowerCase())) throw new Error('Boolean values must be true or false.'); value=raw.toLowerCase()==='true'; }
        else { value=Number(raw); if(raw==='' || !Number.isFinite(value) || (kind==='integer' && (!Number.isSafeInteger(value) || value<0 || value>2147483647))) throw new Error('Enter a finite float or a non-negative 32-bit integer.'); }
        const result=await api('telemetry/'+kind,'POST',{batches:[[{sensorId:$('#ingest-id').value.trim(),sequence:Number($('#ingest-sequence').value),timestamp:new Date().toISOString(),value}]]});
        $('#ingest-result').textContent=JSON.stringify(result,null,2); notify(result.accepted?'Packet accepted.':'Packet rejected. Read the validation details.',!result.accepted); if(result.accepted) $('#ingest-sequence').value=Number($('#ingest-sequence').value)+1;
    });
    on('#power-form','submit',async () => { const d=await api('telemetry/power-comparison','POST',{leftSensorId:$('#power-left').value.trim(),rightSensorId:$('#power-right').value.trim()}); $('#power-result').innerHTML=`<div class="power-cell"><small>COMBINED LOAD</small><strong>${esc(d.totalWatts)} W</strong></div><div class="power-cell"><small>LEFT − RIGHT</small><strong>${esc(d.deltaWatts)} W</strong></div><div><small class="muted">Left: ${esc(d.leftWatts)} W / ${esc(utc(d.leftTime))} UTC</small></div><div><small class="muted">Right: ${esc(d.rightWatts)} W / ${esc(utc(d.rightTime))} UTC</small></div>`; });
    on('#upload-form','submit',async () => {
        const file=$('#attachment').files[0]; if(!file || file.size===0 || file.size>8*1024*1024) throw new Error('Choose a non-empty file no larger than 8 MiB.');
        const data=new FormData(); data.append('file',file); uploadController=new AbortController(); $('#cancel-upload').hidden=false; $('#upload-status').textContent='Uploading and encrypting…';
        try { await api(`sensors/${encodeURIComponent(routeId())}/attachments`,'POST',data,uploadController.signal); $('#upload-status').textContent='Encrypted attachment saved.'; $('#attachment').value=''; await attachments(); notify('Attachment saved securely.'); }
        catch(error){$('#upload-status').textContent=error.message;throw error;}
        finally { $('#cancel-upload').hidden=true; uploadController=null; }
    });
    on('#cancel-upload','click',()=>uploadController?.abort());
    on('#issue-form','submit',async()=>{await api('dashboard/issues','POST',{sensorId:routeId(),description:$('#issue-text').value.trim()});$('#issue-text').value='';notify('Observation added to the issue journal.');});
    on('#validate-tree','click',()=>validateTree(false)); on('#save-tree','click',()=>validateTree(true));
    if(!delegatedBound) { delegatedBound=true; document.addEventListener('click',async e=>{const ack=e.target.closest('[data-ack]'),resolve=e.target.closest('[data-resolve]');try{if(ack){await api(`dashboard/alerts/${ack.dataset.ack}/acknowledge`,'POST',{});await dashboard();}if(resolve){await api(`dashboard/issues/${resolve.dataset.resolve}/resolve`,'POST',{});await issues();}}catch(error){notify(error.message,true);}}); }
}
/** UI-24: refresh sequentially; do not overlap requests or overwrite a user's configuration edits. */
async function refresh() {
    try { if(!document.hidden && !paused) { if(page==='stream') { if(!streamState.paused) await loadStream(); } else if(page==='dashboard') await dashboard(); else if(page==='sensor-details') await details(); else await api('health'); } }
    catch(error){notify(error.message,true);}
    finally { setTimeout(refresh,5000); }
}
/** UI-25: initialise the correct workspace, preserve page-specific forms and establish live availability. */
async function boot() {
    const nav = page.startsWith('sensor') ? 'sensors' : page==='history' ? 'telemetry' : page;
    document.querySelector(`[data-nav="${nav}"]`)?.classList.add('active');
    bind();
    if(page==='stream') bindStream();
    try {
        await api('health');
        if(page==='dashboard') await dashboard();
        else if(page==='sensors') await sensors();
        else if(page==='sensor-create'||page==='sensor-edit') await sensorForm();
        else if(page==='sensor-details') await details();
        else if(page==='history') await telemetryHistory();
        else if(page==='deployment') await deployment();
        else if(page==='issues') await issues();
        else if(page==='stream') await loadStream();
        else if(page==='topology') $('#topology-tree').innerHTML=treeMarkup(await api('deployment'));
    } catch(error) { notify(error.message,true); }
}
window.addEventListener('DOMContentLoaded',()=>{
    const tick=()=>$('#clock').textContent=new Date().toLocaleTimeString('en-GB',{timeZone:'UTC'})+' UTC';tick();setInterval(tick,1000);
    boot();setTimeout(refresh,3000);
});
