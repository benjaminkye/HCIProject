const connectionStatus = document.getElementById('connectionStatus');
const reconnectBtn = document.getElementById('reconnectBtn');
const btnText = document.getElementById('btnText');
const btnLoading = document.getElementById('btnLoading');

let isConnected = false;

function updateConnectionStatus(connected) {
  isConnected = connected;
  
  if (connected) {
    connectionStatus.className = 'status-indicator status-connected';
    connectionStatus.innerHTML = '<span class="status-dot"></span><span>Connected</span>';
    reconnectBtn.textContent = 'Reconnect';
    reconnectBtn.disabled = false;
  } else {
    connectionStatus.className = 'status-indicator status-disconnected';
    connectionStatus.innerHTML = '<span class="status-dot"></span><span>Disconnected</span>';
    reconnectBtn.textContent = 'Connect';
    reconnectBtn.disabled = false;
  }
}

async function checkConnectionStatus() {
  try {
    const response = await chrome.runtime.sendMessage({ type: 'CHECK_CONNECTION' });
    updateConnectionStatus(response?.connected || false);
  } catch (error) {
    console.error('Error checking connection:', error);
    updateConnectionStatus(false);
  }
}

async function attemptReconnect() {
  reconnectBtn.disabled = true;
  btnText.style.display = 'none';
  btnLoading.style.display = 'inline-block';
  
  try {
    await chrome.runtime.sendMessage({ type: 'RECONNECT' });
    
    await new Promise(resolve => setTimeout(resolve, 500));
    
    await checkConnectionStatus();
  } catch (error) {
    console.error('Error reconnecting:', error);
    updateConnectionStatus(false);
  } finally {
    btnText.style.display = 'inline';
    btnLoading.style.display = 'none';
  }
}

reconnectBtn.addEventListener('click', attemptReconnect);

checkConnectionStatus();

setInterval(checkConnectionStatus, 2000);
