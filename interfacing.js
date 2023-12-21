var device;

const data = document.getElementById('data');
const info = document.getElementById('info');

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
    info.innerHTML = '';

    for (let key in versionData) {
      info.innerHTML += `<div><b>${ key }</b>: ${ versionData[key] }</div>`;
    }
    
    document.getElementById('data').innerHTML = `Connected to Playdate ${ serial }`;
    document.getElementById('data').style.color = '#00ff00';

    await device.sendCommand("datadisk");

    device.on('disconnect', () => {
      document.getElementById('data').innerHTML = 'Playdate disconnected. It is either disconnected or is syncing with your computer. When the LED stops flashing, try connecting again.';
      document.getElementById('data').style.color = '#ff0000';
    });
  }
  catch(e) {
    console.warn(e.message);
    document.getElementById('data').innerHTML = 'Error connecting to Playdate, try again';
    document.getElementById('data').style.color = '#ff0000';
  }
}