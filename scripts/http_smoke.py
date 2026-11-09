"""Run against two running local servers. Standard-library only; exits nonzero on failure."""
import json, urllib.request, urllib.error, http.cookiejar, re, time
API='http://127.0.0.1:5267'; UI='http://127.0.0.1:5080'
opener=urllib.request.build_opener(urllib.request.ProxyHandler({}),urllib.request.HTTPCookieProcessor(http.cookiejar.CookieJar()))
def call(base,path,data=None,token=None):
    headers={'Content-Type':'application/json'}
    if token:headers['X-CSRF-TOKEN']=token
    with opener.open(urllib.request.Request(base+path,json.dumps(data).encode() if data is not None else None,headers),timeout=30) as response:return json.load(response)
def check(condition,message):
    assert condition,message
    print('PASS:',message)
first=call(API,'/api/stream');time.sleep(4);second=call(API,'/api/stream')
check(second['queue']['standardProcessed']>first['queue']['standardProcessed'],'simulator crosses HTTP FIFO ingress')
for route in ['/','/Stream','/Dashboard','/Sensors','/Telemetry','/Deployment','/Issues','/Topology']:
    with opener.open(UI+route) as response:
        text=response.read().decode();check(response.status==200 and 'id="workspace"' in text,'MVC route '+route)
with opener.open(UI+'/Stream') as response:text=response.read().decode()
token=re.search(r'name="csrf-token" content="([^"]+)"',text).group(1)
check(call(UI,'/gateway/stream')['totalDevices']==1200,'same-origin proxy reaches API')
try:call(UI,'/gateway/stream/commands',{'sensorId':'SX-0003','enabled':True})
except urllib.error.HTTPError as error:check(error.code==400,'CSRF token required for mutation')
else:raise AssertionError('Missing CSRF protection')
c=call(UI,'/gateway/stream/commands',{'sensorId':'SX-0003','enabled':True},token)
check(call(API,'/api/stream?selected=SX-0003')['selectedOverride'] is True,'confirmed override reaches API')
call(UI,'/gateway/stream/undo/'+c['id'],{},token)
check(call(API,'/api/stream?selected=SX-0003')['selectedOverride'] is None,'HTTP undo restores automatic mode')
print('HTTP SMOKE COMPLETE')
