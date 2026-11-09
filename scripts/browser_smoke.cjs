/* Optional UI verification: install Playwright separately; keep both servers running. */
const {chromium}=require('playwright');
const assert=require('node:assert/strict');
(async()=>{
 const browser=await chromium.launch({headless:true,...(process.env.SMARTX_BROWSER?{executablePath:process.env.SMARTX_BROWSER}:{})});
 try {
  const p=await browser.newPage({viewport:{width:1440,height:1100}}),errors=[];
  p.on('pageerror',e=>errors.push(e.message));
  await p.goto(process.env.SMARTX_UI||'http://127.0.0.1:5080/Stream');
  await p.waitForSelector('#stream-devices .fleet-device');
  await p.locator('[data-device="SX-0003"]').first().click();
  await p.waitForFunction(()=>document.querySelector('#stream-device-name').textContent.includes('0003'));
  await p.locator('#command-open').click();await p.locator('#command-confirm').click();
  await p.waitForFunction(()=>document.querySelector('#stream-history').textContent.includes('ISSUED'));
  await p.locator('#command-undo').click();await p.waitForFunction(()=>document.querySelector('#stream-history').textContent.includes('UNDONE'));
  console.log('PASS: confirmed command and undo');
  await p.locator('#stream-search').fill('irrigation');
  await p.locator('[data-nav="telemetry"]').click();await p.waitForSelector('#ingest-form');
  await p.waitForFunction(()=>!navigating);await p.locator('#ingest-id').fill('MY-DRAFT');
  await p.locator('[data-nav="stream"]').click();await p.waitForSelector('#stream-search');await p.waitForFunction(()=>!navigating);
  assert.equal(await p.locator('#stream-search').inputValue(),'irrigation');
  await p.goBack();await p.waitForSelector('#ingest-form');await p.waitForFunction(()=>!navigating);
  assert.equal(await p.locator('#ingest-id').inputValue(),'MY-DRAFT');
  await p.goForward();await p.waitForSelector('#stream-search');await p.waitForFunction(()=>!navigating);
  console.log('PASS: async routes, back/forward, search and draft preservation');
  await p.locator('#stream-spike').click();await p.waitForFunction(()=>document.querySelector('#stream-lab-result').textContent.includes('1 accepted'));
  await p.locator('#stream-trend').click();await p.waitForFunction(()=>document.querySelector('#stream-lab-result').textContent.includes('6 accepted'));
  await p.waitForFunction(()=>document.querySelector('#stream-insights').textContent.includes('Trend forecast'));
  assert.match(await p.locator('#stream-insights').innerText(),/TREND FORECAST/);
  console.log('PASS: real HTTP spike and trend experiment with forecast');
  await p.setViewportSize({width:390,height:844});assert.equal(await p.evaluate(()=>document.documentElement.scrollWidth>innerWidth),false);
  assert.deepEqual(errors,[]);console.log('PASS: mobile containment and no uncaught browser errors');
 } finally {await browser.close();}
})().catch(e=>{console.error(e);process.exit(1)});
