/* Async same-origin workspace routing. Preserve unfinished form values and view choices per URL. */
const workspaceState=new Map();let navigating=false,activePath=location.pathname+location.search;
function rememberWorkspace() {
    const fields={};document.querySelectorAll('#workspace input[id],#workspace select[id],#workspace textarea[id]').forEach(el=>{if(el.type!=='file')fields[el.id]={value:el.value,checked:el.checked};});
    workspaceState.set(activePath,{fields,scroll:window.scrollY,paused,sensorPage,streamState:{...streamState}});
}
async function navigateWorkspace(url,pop=false) {
    if(navigating)return;
    if(uploadController){notify('Finish or cancel the current upload before changing workspaces.',true);return;}
    navigating=true;rememberWorkspace();
    try {
        const response=await fetch(url,{headers:{'X-Workspace-Navigation':'true'}});
        if(!response.ok)throw new Error('This workspace could not be loaded. Please retry.');
        const doc=new DOMParser().parseFromString(await response.text(),'text/html'),main=doc.querySelector('#workspace');
        if(!main)throw new Error('The server returned an invalid workspace.');
        routeGeneration++;$('#workspace').removeEventListener('click',streamClick);
        $('#workspace').replaceChildren(...main.childNodes);document.title=doc.title;
        document.body.dataset.page=page=doc.body.dataset.page;
        const csrf=doc.querySelector('meta[name="csrf-token"]');if(csrf)$('meta[name="csrf-token"]').content=csrf.content;
        document.querySelector('.topbar>span').innerHTML=doc.querySelector('.topbar>span').innerHTML;
        document.querySelectorAll('[data-nav]').forEach(el=>el.classList.remove('active'));
        activePath=new URL(url,location.origin).pathname+new URL(url,location.origin).search;
        if(!pop)window.history.pushState({},'',activePath);
        const saved=workspaceState.get(activePath);paused=saved?.paused??false;sensorPage=saved?.sensorPage??1;
        if(saved?.streamState && page==='stream')streamState={...saved.streamState};
        const restore=()=>{if(saved)for(const [id,value] of Object.entries(saved.fields)){const el=document.getElementById(id);if(el){el.value=value.value;el.checked=value.checked;}}};
        restore();await boot();restore();
        if(page==='stream')await loadStream();
        $('#pause-view') && ($('#pause-view').textContent=paused?'Resume view':'Pause view');
        window.scrollTo(0,saved?.scroll??0);
        const heading=$('#workspace h1');if(heading){heading.tabIndex=-1;heading.focus({preventScroll:true});}
    }catch(error){notify(error.message,true);}finally{navigating=false;}
}
document.addEventListener('click',e=>{
    const a=e.target.closest('a[href]');if(!a||e.defaultPrevented||e.button!==0||e.metaKey||e.ctrlKey||e.shiftKey||e.altKey||a.target||a.download)return;
    const url=new URL(a.href,location.href);
    if(url.origin!==location.origin||url.hash||url.pathname.startsWith('/gateway/')||!/^\/(?:$|Dashboard\/?$|Stream\/?$|Topology\/?$|Sensors(?:\/.*)?$|Telemetry(?:\/.*)?$|Deployment\/?$|Issues\/?$)/i.test(url.pathname))return;
    e.preventDefault();navigateWorkspace(url.pathname+url.search);
});
window.addEventListener('popstate',()=>navigateWorkspace(location.pathname+location.search,true));
