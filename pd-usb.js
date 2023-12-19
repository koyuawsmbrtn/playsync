/*!!
pd-usb v2.0.1
JavaScript library for interacting with a Panic Playdate console over USB
https://github.com/jaames/pd-usb
2022 James Daniel
Playdate is (c) Panic Inc. - this project isn't affiliated with or endorsed by them in any way
*/
!function(t,e){"object"==typeof exports&&"undefined"!=typeof module?e(exports):"function"==typeof define&&define.amd?define(["exports"],e):e((t="undefined"!=typeof globalThis?globalThis:t||self).pdusb={})}(this,(function(t){"use strict";function e(t,e,s,i){return new(s||(s=Promise))((function(n,o){function r(t){try{l(i.next(t))}catch(t){o(t)}}function a(t){try{l(i.throw(t))}catch(t){o(t)}}function l(t){var e;t.done?n(t.value):(e=t.value,e instanceof s?e:new s((function(t){t(e)}))).then(r,a)}l((i=i.apply(t,e||[])).next())}))}function s(t,e="Assert failed"){t||function(t){throw console.trace(t),new Error(t)}(e)}"undefined"!=typeof process&&null!=process.versions&&process.versions.node;const i=/^\s*([^=]+?)\s*=\s*(.*?)\s*$/;function n(t){const e=new Uint8Array(t.length);for(let s=0;s<t.length;s++)e[s]=t.charCodeAt(s);return e}function o(t){const e=t.reduce(((t,e)=>t+e.length),0),s=new Uint8Array(e);for(let e=0,i=0;i<t.length;i++)s.set(t[i],e),e+=t[i].length;return s}class r{constructor(){this.mode=1,this.lineBuffer="",this.asciiDecoder=new TextDecoder("ascii"),this.bytePackets=[],this.bytesReceived=0,this.bytesTarget=0,this.dataReceivedCallback=t=>{}}onData(t){this.dataReceivedCallback=t}setMode(t){this.mode!==t&&(1===t?this.lineBuffer="":0===t&&(this.bytePackets=[],this.bytesReceived=0,this.bytesTarget=0),this.mode=t)}clearOut(){1===this.mode?this.controller.enqueue(""):0===this.mode&&this.controller.enqueue(new Uint8Array(0))}start(t){this.controller=t}transform(t,e){1===this.mode?this.transformLines(t,e):0===this.mode&&this.transformBytes(t,e),this.dataReceivedCallback(t)}flush(t){1===this.mode?this.flushLines(t):0===this.mode&&this.flushBytes(t)}transformLines(t,e){this.lineBuffer+=this.asciiDecoder.decode(t);const s=this.lineBuffer.split(/\r?\n/);this.lineBuffer=s.pop(),s.forEach((t=>e.enqueue(t)))}transformBytes(t,e){this.bytesTarget>0?(this.bytePackets.push(t),this.bytesReceived+=t.byteLength,this.bytesReceived>=this.bytesTarget&&(e.enqueue(o(this.bytePackets)),this.bytePackets=[],this.bytesReceived=0)):e.enqueue(t)}flushLines(t){t.enqueue(this.lineBuffer),this.lineBuffer=""}flushBytes(t){}}class a{constructor(t,e){this.errorCallback=()=>{},this.isOpen=!1,this.isReading=!1,this.isWaitingForRead=!1,this.handleDisconnectEvent=()=>{this.isOpen=!1,this.isReading=!1},this.port=t,this.options=e,t.addEventListener("disconnect",this.handleDisconnectEvent)}onData(t){this.readTransformer.onData(t)}onError(t){this.errorCallback=t}open(){return e(this,void 0,void 0,(function*(){yield this.port.open(Object.assign({},this.options)),this.readTransformer=new r;const t=this.port.readable.pipeThrough(new TransformStream(this.readTransformer));this.reader=t.getReader(),this.isOpen=!0}))}close(){return e(this,void 0,void 0,(function*(){yield this.reader.cancel(),yield this.port.close(),this.isOpen=!1}))}write(t){return e(this,void 0,void 0,(function*(){s(this.isOpen,"Serial is not open, please call open() before beginning to write data");const e=this.port.writable.getWriter();yield e.write(t),e.releaseLock()}))}writeAscii(t){return e(this,void 0,void 0,(function*(){const e=n(t);return yield this.write(e)}))}readBytes(t=0){return e(this,void 0,void 0,(function*(){s(!this.isReading,"A read was queued while another was still in progress"),this.isReading=!0;try{this.readTransformer.setMode(0),this.readTransformer.bytesTarget=t;const{value:e}=yield this.reader.read();return this.readTransformer.bytesTarget=0,this.isReading=!1,e}catch(t){this.isReading=!1,this.errorCallback(t)}}))}clearRead(){return e(this,void 0,void 0,(function*(){this.port.readable&&this.isWaitingForRead&&(this.readTransformer.clearOut(),this.isWaitingForRead=!1)}))}readLine(){return e(this,void 0,void 0,(function*(){s(!this.isReading,"A read was queued while another was still in progress"),this.isReading=!0;try{this.readTransformer.setMode(1);const{value:t}=yield this.reader.read();return this.isReading=!1,t}catch(t){this.isReading=!1,this.errorCallback(t)}}))}doReadWithTimeout(t,s,...i){return e(this,void 0,void 0,(function*(){return new Promise(((e,n)=>{this.isWaitingForRead=!0;const o=setTimeout((()=>{e({value:null,done:!0}),this.clearRead()}),t);try{s.bind(this)(...i).then((t=>{clearTimeout(o),this.isWaitingForRead=!1,e({value:t,done:!1})}))}catch(t){clearTimeout(o),n(t)}}))}))}readLinesUntilTimeout(t=50){return e(this,void 0,void 0,(function*(){const e=[];for(;this.isOpen&&this.port.readable;)try{const{value:s,done:i}=yield this.doReadWithTimeout(t,this.readLine);if(null!==s&&e.push(s),i)return e}catch(t){this.errorCallback(t)}}))}readBytesUntilTimeout(t=50){return e(this,void 0,void 0,(function*(){const e=[];for(;this.isOpen&&this.port.readable;)try{const{value:s,done:i}=yield this.doReadWithTimeout(t,this.readBytes);if(null!==s&&e.push(s),i)return o(e)}catch(t){this.errorCallback(t)}}))}}const l={1:"left",2:"right",4:"up",8:"down",16:"a",32:"b",64:"menu",128:"lock"},d={left:1,right:2,up:4,down:8,a:16,b:32,menu:64,lock:128},h=4913,u=22336,c={usbVendorId:h,usbProductId:u},g=115200;class f{constructor(t){this.isConnected=!0,this.isPollingControls=!1,this.isStreaming=!1,this.lastButtonPressedFlags=0,this.lastButtonJustReleasedFlags=0,this.lastButtonJustPressedFlags=0,this.logCommandResponse=!1,this.events={},this.handleDisconnectEvent=()=>{this.isConnected=!1,this.emit("disconnect")},this.handleSerialDataEvent=t=>{this.emit("data",t)},this.handleSerialErrorEvent=t=>{throw this.emit("error",t),t},this.port=t,this.serial=new a(t,{baudRate:g,bufferSize:12800}),this.port.addEventListener("disconnect",this.handleDisconnectEvent)}static requestDevice(){return e(this,void 0,void 0,(function*(){const t=yield navigator.serial.requestPort({filters:[c]});return new f(t)}))}static getDevices(){return e(this,void 0,void 0,(function*(){return(yield navigator.serial.getPorts()).filter((t=>{const{usbProductId:e,usbVendorId:s}=t.getInfo();return e===u&&s===h})).map((t=>new f(t)))}))}get isOpen(){return this.isConnected&&this.serial.isOpen}get isBusy(){return this.isConnected&&(this.isPollingControls||this.isStreaming)}on(t,e){(this.events[t]||(this.events[t]=[])).push(e)}off(t,e){const s=this.events[t];s&&s.splice(s.indexOf(e),1)}emit(t,...e){(this.events[t]||[]).forEach((t=>t.apply(this,e)))}open(){return e(this,void 0,void 0,(function*(){yield this.serial.open(),this.serial.onData(this.handleSerialDataEvent),this.serial.onError(this.handleSerialErrorEvent);const t=yield this.sendCommand("echo off");s(Array.isArray(t),"Open error - Playdate did not respond, maybe something else is interacting with it?");const e=t.pop();s(""===e||e.includes("echo off"),`Open error - invalid echo command response, got ${e}`),this.emit("open")}))}close(){return e(this,void 0,void 0,(function*(){this.isPollingControls&&(yield this.stopPollingControls()),yield this.serial.close(),this.emit("close")}))}getVersion(){return e(this,void 0,void 0,(function*(){const t=yield this.sendCommand("version"),e={};return t.forEach((t=>{const s=function(t){if(i.test(t)){const e=t.match(i);return[e[1],e[2]]}return t}(t);Array.isArray(s)&&(e[s[0]]=s[1])})),s("SDK"in e),s("boot_build"in e),s("build"in e),s("cc"in e),s("pdxversion"in e),s("serial#"in e),s("target"in e),{sdk:e.SDK,bootBuild:e.boot_build,build:e.build,cc:e.cc,pdxVersion:e.pdxversion,serial:e["serial#"],target:e.target}}))}getSerial(){return e(this,void 0,void 0,(function*(){return(yield this.sendCommand("serialread")).find((t=>""!==t))}))}getScreen(){return e(this,void 0,void 0,(function*(){this.assertNotBusy(),yield this.serial.writeAscii("screen\n");const t=yield this.serial.readBytes(12011);s(t.byteLength>=12011,`Screen command response is too short, got ${t.byteLength} bytes`);const e=n("~screen:\n");let i=function(t,e,s=0){t:for(;;){let i=t.indexOf(e[0],s);if(-1===i)return-1;s=i;for(let i=1;i<e.length;i++)if(t[s+i]!==e[i]){s+=1;continue t}return s}}(t,e);s(-1!==i,"Invalid screen command response"),i+=e.length;return t.subarray(i,i+12e3)}))}getScreenIndexed(){return e(this,void 0,void 0,(function*(){const t=yield this.getScreen(),e=t.byteLength,s=new Uint8Array(96e3);let i=0,n=0;for(;i<e;){const e=t[i++];for(let t=7;t>=0;t--)s[n++]=e>>t&1}return s}))}sendBitmap(t){return e(this,void 0,void 0,(function*(){this.assertNotBusy(),s(12e3===t.length,`Bitmap size is incorrect; should be 12000 (400 * 240 / 8), got ${t.length}`);const e=new Uint8Array(12007);e.set(n("bitmap\n"),0),e.set(t,7),yield this.serial.write(e);const[i]=yield this.serial.readLinesUntilTimeout();s(""===i,`Invalid bitmap send response, got ${i}`)}))}sendBitmapIndexed(t){return e(this,void 0,void 0,(function*(){s(96e3===t.length,`Bitmap size is incorrect; should be 96000 (400 * 240), got ${t.length}`);const e=new Uint8Array(12e3),i=e.byteLength;let n=0,o=0;for(;o<i;){let s=0;for(let e=0;e<8;e++)0!==t[n++]&&(s|=128>>e);e[o++]=s}yield this.sendBitmap(e)}))}startPollingControls(){return e(this,void 0,void 0,(function*(){this.assertNotBusy(),yield this.serial.writeAscii("buttons\n"),this.emit("controls:start"),this.isPollingControls=!0,this.pollControlsLoop()}))}getControls(){return s(this.isPollingControls,"Please begin polling Playdate controls by calling startPollingControls() first"),this.lastControlState}buttonIsPressed(t){return s(this.isPollingControls,"Please begin polling Playdate controls by calling startPollingControls() first"),"string"==typeof t&&(t=d[t]),0!=(this.lastButtonPressedFlags&t)}buttonJustPressed(t){return s(this.isPollingControls,"Please begin polling Playdate controls by calling startPollingControls() first"),"string"==typeof t&&(t=d[t]),0!=(this.lastButtonJustPressedFlags&t)}buttonJustReleased(t){return s(this.isPollingControls,"Please begin polling Playdate controls by calling startPollingControls() first"),"string"==typeof t&&(t=d[t]),0!=(this.lastButtonJustReleasedFlags&t)}isCrankDocked(){return s(this.isPollingControls,"Please begin polling Playdate controls by calling startPollingControls() first"),this.lastControlState.crankDocked}getCrankPosition(){return s(this.isPollingControls,"Please begin polling Playdate controls by calling startPollingControls() first"),this.lastControlState.crank}stopPollingControls(){return e(this,void 0,void 0,(function*(){s(this.isPollingControls,"Controls are not currently being polled"),yield this.serial.writeAscii("\n"),this.lastControlState=void 0,this.lastButtonPressedFlags=0,this.lastButtonJustReleasedFlags=0,this.lastButtonJustPressedFlags=0,this.isPollingControls=!1}))}run(t){return e(this,void 0,void 0,(function*(){s(t.startsWith("/"),'Path must begin with a forward slash, e.g. "/System/Crayons.pdx"');const[e]=yield this.sendCommand(`run ${t}`);s(""===e,`Invalid run response, got ${e}`)}))}evalLuaPayload(t,s=50){return e(this,void 0,void 0,(function*(){const e=`eval ${t.byteLength}\n`,i=new Uint8Array(e.length+t.byteLength);return i.set(n(e),0),i.set(new Uint8Array(t),e.length),yield this.serial.write(i),yield this.serial.readLinesUntilTimeout(s)}))}sendCommand(t){return e(this,void 0,void 0,(function*(){s(this.isOpen),this.assertNotBusy(),yield this.serial.writeAscii(`${t}\n`);const e=yield this.serial.readLinesUntilTimeout();return this.logCommandResponse&&console.log(e.join("\n")),e}))}pollControlsLoop(){return e(this,void 0,void 0,(function*(){for(;this.isPollingControls&&this.port.readable;)try{const t=yield this.serial.readLine(),e=this.parseControlState(t);e&&this.emit("controls:update",e)}catch(t){if(this.isPollingControls)throw t}this.emit("controls:stop")}))}assertNotBusy(){s(!this.isBusy,"Device is currently busy, stop polling controls or streaming to send further commands")}parseControlState(t){const e=t.match(/buttons:([A-F0-9]{2}) ([A-F0-9]{2}) ([A-F0-9]{2}) crank:(\d+\.?\d+) docked:(\d)/);if(e){const t=parseInt(e[1],16),s=parseInt(e[2],16),i=parseInt(e[3],16),n={crank:parseFloat(e[4]),crankDocked:"1"===e[5],pressed:this.parseButtonFlags(t),justPressed:this.parseButtonFlags(s),justReleased:this.parseButtonFlags(i)};return this.lastButtonPressedFlags=t,this.lastButtonJustPressedFlags=s,this.lastButtonJustReleasedFlags=i,this.lastControlState=n,n}}parseButtonFlags(t){return{[l[2]]:0!=(2&t),[l[1]]:0!=(1&t),[l[4]]:0!=(4&t),[l[8]]:0!=(8&t),[l[16]]:0!=(16&t),[l[32]]:0!=(32&t),[l[64]]:0!=(64&t),[l[128]]:0!=(128&t)}}}function p(){s(window.isSecureContext,"Web Serial is only supported in secure contexts\nhttps://developer.mozilla.org/en-US/docs/Web/Security/Secure_Contexts"),s(void 0!==navigator.serial,"Web Serial is not supported by this browser.\nhttps://developer.mozilla.org/en-US/docs/Web/API/Web_Serial_API#browser_compatibility")}t.PDButtonBitmaskMap=d,t.PDButtonNameMap=l,t.PDSerial=a,t.PLAYDATE_BAUDRATE=g,t.PLAYDATE_BUFFER_SIZE=12800,t.PLAYDATE_HEIGHT=240,t.PLAYDATE_PID=u,t.PLAYDATE_VID=h,t.PLAYDATE_WIDTH=400,t.PlaydateDevice=f,t.USB_FILTER=c,t.assertUsbSupported=p,t.isUsbSupported=function(){return window.isSecureContext&&void 0!==navigator.serial},t.kButtonA=32,t.kButtonB=16,t.kButtonDown=8,t.kButtonLeft=1,t.kButtonLock=128,t.kButtonMenu=64,t.kButtonRight=2,t.kButtonUp=4,t.requestConnectPlaydate=function(){return e(this,void 0,void 0,(function*(){return p(),yield f.requestDevice()}))},t.version="2.0.1",Object.defineProperty(t,"__esModule",{value:!0})}));
