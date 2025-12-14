let ws = null;
let reconnectInterval = null;
let isConnected = false;
let isConnecting = false;
let isManualReconnect = false;
let zoomEnabled = false;
let currentFontSize = 'Medium';
let hasLoggedDisconnection = false;

const WS_URL = 'ws://localhost:9876';
const RECONNECT_DELAY = 5000;

const ZOOM_LEVELS = {
  'Small': 0.9,
  'Medium': 1.0,
  'Large': 1.15,
  'Extra Large': 1.25
};

function connectWebSocket() {
  if (isConnecting || (ws && ws.readyState === WebSocket.CONNECTING)) {
    return;
  }
  
  if (ws && ws.readyState === WebSocket.OPEN && !isManualReconnect) {
    return;
  }
  
  try {
    isConnecting = true;
    ws = new WebSocket(WS_URL);
    
    ws.onopen = () => {
      console.log('✓ Connected to Digital Helper');
      isConnected = true;
      isConnecting = false;
      hasLoggedDisconnection = false;
      
      if (reconnectInterval) {
        clearInterval(reconnectInterval);
        reconnectInterval = null;
      }
      
      sendMessage({
        type: 'CONNECTION_STATUS',
        connected: true
      });
    };
    
    ws.onmessage = async (event) => {
      try {
        const message = JSON.parse(event.data);
        await handleMessage(message);
      } catch (error) {
        console.error('Error handling message:', error);
      }
    };
    
    ws.onerror = (error) => {
      isConnecting = false;
      if (!hasLoggedDisconnection) {
        console.log('Digital Helper app not running. Extension will connect automatically when app starts.');
        hasLoggedDisconnection = true;
      }
    };
    
    ws.onclose = (event) => {
      isConnecting = false;
      const wasManualReconnect = isManualReconnect;
      isManualReconnect = false;
      
      console.log(`WebSocket closed - Code: ${event.code}, Reason: ${event.reason || 'none'}, Clean: ${event.wasClean}`);
      
      if (isConnected && !hasLoggedDisconnection && !wasManualReconnect) {
        console.log('✗ Disconnected from Digital Helper. Will attempt to reconnect...');
        hasLoggedDisconnection = true;
      }
      
      isConnected = false;
      ws = null;
      
      if (wasManualReconnect) {
        console.log('Manual reconnect - attempting new connection...');
        setTimeout(() => connectWebSocket(), 200);
        return;
      }
      
      if (!reconnectInterval) {
        reconnectInterval = setInterval(connectWebSocket, RECONNECT_DELAY);
      }
    };
    
  } catch (error) {
    isConnecting = false;
    if (!reconnectInterval) {
      reconnectInterval = setInterval(connectWebSocket, RECONNECT_DELAY);
    }
  }
}

async function handleMessage(message) {
  switch (message.type) {
    case 'REQUEST_DOM':
      await requestDomFromContentScript(message.tabId);
      break;
      
    case 'HIGHLIGHT_ELEMENT':
      await highlightElementInContentScript(message.tabId, message.selector, message.color, message.thickness);
      break;
      
    case 'CLEAR_HIGHLIGHT':
      await clearHighlightInContentScript(message.tabId);
      break;
      
    case 'SET_ZOOM':
      await setZoom(message.fontSize, message.enabled);
      break;
      
    case 'SET_ZOOM_ENABLED':
      zoomEnabled = message.enabled;
      break;
      
    case 'PING':
      sendMessage({ type: 'PONG' });
      break;
      
    default:
      console.warn('Unknown message type:', message.type);
  }
}

chrome.runtime.onMessage.addListener((request, sender, sendResponse) => {
  if (request.type === 'CHECK_CONNECTION') {
    sendResponse({ connected: isConnected });
    return true;
  } else if (request.type === 'RECONNECT') {
    isManualReconnect = true;
    
    if (ws && ws.readyState === WebSocket.OPEN) {
      console.log('Manual reconnect - closing existing connection...');
      ws.close();
    } else {
      connectWebSocket();
    }
    
    sendResponse({ success: true });
    return true;
  }
});

async function requestDomFromContentScript(tabId) {
  try {
    if (!tabId) {
      const tabs = await chrome.tabs.query({ active: true, currentWindow: true });
      if (tabs.length === 0) {
        sendMessage({ type: 'ERROR', message: 'No active tab found' });
        return;
      }
      tabId = tabs[0].id;
    }
    
    const response = await chrome.tabs.sendMessage(tabId, { type: 'REQUEST_DOM' });
    
    if (response && response.type === 'DOM_SUMMARY') {
      sendMessage({
        type: 'DOM_SUMMARY',
        data: response.data,
        tabId: tabId
      });
    }
  } catch (error) {
    if (!error.message.includes('Could not establish connection') && 
        !error.message.includes('Receiving end does not exist')) {
      console.error('Error requesting DOM:', error);
    }
    sendMessage({ 
      type: 'ERROR', 
      message: 'Failed to extract DOM: ' + error.message 
    });
  }
}

async function highlightElementInContentScript(tabId, selector, color, thickness) {
  try {
    if (!tabId) {
      const tabs = await chrome.tabs.query({ active: true, currentWindow: true });
      if (tabs.length === 0) {
        sendMessage({ type: 'ERROR', message: 'No active tab found' });
        return;
      }
      tabId = tabs[0].id;
    }
    
    await chrome.tabs.sendMessage(tabId, { 
      type: 'HIGHLIGHT_ELEMENT', 
      selector: selector,
      color: color,
      thickness: thickness
    });
    
    sendMessage({ type: 'HIGHLIGHT_SUCCESS' });
  } catch (error) {
    if (!error.message.includes('Could not establish connection') && 
        !error.message.includes('Receiving end does not exist')) {
      console.error('Error highlighting element:', error);
    }
    sendMessage({ 
      type: 'ERROR', 
      message: 'Failed to highlight element: ' + error.message 
    });
  }
}

async function clearHighlightInContentScript(tabId) {
  try {
    if (!tabId) {
      const tabs = await chrome.tabs.query({ active: true, currentWindow: true });
      if (tabs.length === 0) return;
      tabId = tabs[0].id;
    }
    
    await chrome.tabs.sendMessage(tabId, { type: 'CLEAR_HIGHLIGHT' });
  } catch (error) {
  }
}

async function setZoom(fontSize, enabled) {
  if (enabled !== undefined) {
    zoomEnabled = enabled;
  }
  
  if (fontSize) {
    currentFontSize = fontSize;
  }
  
  if (!zoomEnabled || !fontSize) {
    return;
  }
  
  const zoomLevel = ZOOM_LEVELS[fontSize];
  if (!zoomLevel) {
    console.warn('Unknown font size:', fontSize);
    return;
  }
  
  try {
    const tabs = await chrome.tabs.query({});
    
    for (const tab of tabs) {
      if (tab.url && (tab.url.startsWith('http://') || tab.url.startsWith('https://'))) {
        await chrome.tabs.setZoom(tab.id, zoomLevel);
      }
    }
    
    console.log(`Set zoom to ${zoomLevel * 100}% for font size: ${fontSize}`);
    
    sendMessage({ 
      type: 'ZOOM_SUCCESS', 
      fontSize: fontSize, 
      zoomLevel: zoomLevel 
    });
  } catch (error) {
    console.error('Error setting zoom:', error);
    sendMessage({ 
      type: 'ERROR', 
      message: 'Failed to set zoom: ' + error.message 
    });
  }
}

function sendMessage(message) {
  if (ws && ws.readyState === WebSocket.OPEN) {
    try {
      ws.send(JSON.stringify(message));
    } catch (error) {
      console.error('Error sending message:', error);
    }
  }
}

async function applyZoomToTab(tabId, url) {
  if (!zoomEnabled || !currentFontSize) {
    return;
  }
  
  const zoomLevel = ZOOM_LEVELS[currentFontSize];
  if (!zoomLevel) {
    return;
  }
  
  if (url && (url.startsWith('http://') || url.startsWith('https://'))) {
    try {
      await chrome.tabs.setZoom(tabId, zoomLevel);
      console.log(`Applied zoom ${zoomLevel * 100}% to new tab`);
    } catch (error) {
      console.debug('Could not apply zoom to tab:', error.message);
    }
  }
}

chrome.tabs.onUpdated.addListener(async (tabId, changeInfo, tab) => {
  if (changeInfo.status === 'complete') {
    await applyZoomToTab(tabId, tab.url);
  }
});

chrome.tabs.onCreated.addListener(async (tab) => {
  if (tab.pendingUrl) {
    await applyZoomToTab(tab.id, tab.pendingUrl);
  } else if (tab.url) {
    await applyZoomToTab(tab.id, tab.url);
  }
});

console.log('Digital Helper Browser Bridge - Ready');
console.log('Waiting for Digital Helper app to start...');
connectWebSocket();
