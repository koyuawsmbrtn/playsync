var device;

const data = document.getElementById('data');

// Check WebSerial support, display error if not supported
try {
  pdusb.assertUsbSupported();
}
catch (e) {
  document.getElementById('data').innerHTML = e.message;
  document.getElementById('connectButton').disabled = true;
}

async function connect() {
  try {
    device = await pdusb.requestConnectPlaydate();
      
    await device.open();
    const serial = await device.getSerial();

    const versionData = await device.getVersion();
    data.innerHTML = '';

    for (let key in versionData) {
      data.innerHTML += `<div><b>${ key }</b>: ${ versionData[key] }</div>`;
    }
    
    document.getElementById('data').innerHTML = `Connected to Playdate ${ serial }`;

    device.on('disconnect', () => {
      document.getElementById('data').innerHTML = 'Playdate disconnected';
    });
  }
  catch(e) {
    console.warn(e.message);
    document.getElementById('data').innerHTML = 'Error connecting to Playdate, try again';
  }
}