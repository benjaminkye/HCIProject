let highlightOverlay = null;
let currentHighlightedElement = null;

chrome.runtime.onMessage.addListener((message, sender, sendResponse) => {
  if (message.type === 'REQUEST_DOM') {
    const domSummary = extractDomSummary();
    sendResponse({ type: 'DOM_SUMMARY', data: domSummary });
  } else if (message.type === 'HIGHLIGHT_ELEMENT') {
    highlightElement(message.selector, message.color, message.thickness);
    sendResponse({ type: 'HIGHLIGHT_SUCCESS' });
  } else if (message.type === 'CLEAR_HIGHLIGHT') {
    clearHighlight();
    sendResponse({ type: 'CLEAR_SUCCESS' });
  }
  return true;
});

function extractDomSummary() {
  const elements = [];
  let elementIdCounter = 0;

  const containerSelectors = [
    'form', 'section', 'article', 'header', 'nav', 'aside', 'footer', 'main',
    '[role="form"]', '[role="search"]', '[role="navigation"]', '[role="main"]', '[role="banner"]', '[role="contentinfo"]',
    'div.login', 'div.signup', 'div.checkout', 'div.search', 'div.modal', 'div.dialog',
    '*[class*="navigation"]', '*[class*="-nav"]', '*[class*="toolbar"]', '*[class*="menubar"]'
  ];
  const containers = document.querySelectorAll(containerSelectors.join(', '));
  
  const interactiveSelectors = [
    'button', 'a', 'input', 'select', 'textarea', 'label',
    '[role="button"]', '[role="link"]', '[role="checkbox"]', '[role="radio"]', '[role="textbox"]',
    '[tabindex="0"]', '[onclick]',
    '*[class*="nav"]', '*[class*="menu"]', '*[class*="item"]', '*[class*="link"]', '*[class*="button"]',
    'span[onclick]', 'div[onclick]', 'span[class*="button"]', 'div[class*="button"]',
    'span[class*="link"]', 'div[class*="link"]', 'span[class*="item"]', 'div[class*="item"]'
  ];
  const interactiveElements = document.querySelectorAll(interactiveSelectors.join(', '));

  containers.forEach(container => {
    if (!isVisible(container)) return;
    
    const containerData = processElement(container, elementIdCounter++);
    if (containerData) {
      const children = [];
      const childSelectors = [
        'button', 'a', 'input', 'select', 'textarea', 'label',
        '[role="button"]', '[role="link"]', '[role="checkbox"]', '[role="radio"]', '[role="textbox"]',
        '[onclick]',
        '*[class*="nav"]', '*[class*="menu"]', '*[class*="item"]', '*[class*="link"]', '*[class*="button"]',
        'span[onclick]', 'div[onclick]', 'span[class*="button"]', 'div[class*="button"]',
        'span[class*="link"]', 'div[class*="link"]', 'span[class*="item"]', 'div[class*="item"]'
      ];
      const childElements = container.querySelectorAll(childSelectors.join(', '));
      
      childElements.forEach(child => {
        if (!isVisible(child)) return;
        if (!isInteractive(child)) return;
        const childData = processElement(child, elementIdCounter++);
        if (childData) {
          children.push(childData);
        }
      });
      
      containerData.children = children;
      elements.push(containerData);
    }
  });

  interactiveElements.forEach(element => {
    if (!isVisible(element)) return;
    
    if (!isInteractive(element)) return;
    
    let isInContainer = false;
    for (const container of containers) {
      if (container.contains(element) && container !== element) {
        isInContainer = true;
        break;
      }
    }
    
    if (!isInContainer) {
      const elementData = processElement(element, elementIdCounter++);
      if (elementData) {
        elements.push(elementData);
      }
    }
  });

  return {
    url: window.location.href,
    title: document.title,
    elements: elements
  };
}

function processElement(element, id) {
  const rect = element.getBoundingClientRect();
  
  if (rect.width === 0 || rect.height === 0) {
    return null;
  }

  const selector = generateSelector(element);
  const tag = element.tagName.toLowerCase();
  const text = getElementText(element);
  const type = element.type || null;
  const placeholder = element.placeholder || null;
  const ariaLabel = element.getAttribute('aria-label') || null;
  const role = element.getAttribute('role') || null;

  return {
    id: `elem_${id}`,
    selector: selector,
    tag: tag,
    text: text,
    type: type,
    placeholder: placeholder,
    ariaLabel: ariaLabel,
    role: role,
    rect: {
      x: Math.round(rect.left + window.scrollX),
      y: Math.round(rect.top + window.scrollY),
      width: Math.round(rect.width),
      height: Math.round(rect.height)
    },
    children: []
  };
}

function isVisible(element) {
  const style = window.getComputedStyle(element);
  const rect = element.getBoundingClientRect();
  
  if (style.display === 'none' || style.visibility === 'hidden') {
    return false;
  }

  if (parseFloat(style.opacity) < 0.1) {
    return false;
  }
  
  if (rect.width === 0 && rect.height === 0) {
    return false;
  }
  
  return true;
}

function isInteractive(element) {
  const style = window.getComputedStyle(element);
  const tag = element.tagName.toLowerCase();
  
  if (['button', 'a', 'input', 'select', 'textarea', 'label'].includes(tag)) {
    return true;
  }
  
  if (element.getAttribute('role') || element.getAttribute('tabindex') || element.onclick) {
    return true;
  }
  
  if (style.cursor === 'pointer') {
    return true;
  }
  
  if (tag.includes('-')) {
    return true;
  }
  
  return false;
}

function getElementText(element) {
  if (element.tagName === 'INPUT' || element.tagName === 'SELECT' || element.tagName === 'TEXTAREA') {
    return element.value || element.placeholder || null;
  }
  
  let text = '';
  for (const node of element.childNodes) {
    if (node.nodeType === Node.TEXT_NODE) {
      text += node.textContent;
    }
  }
  
  text = text.trim();
  if (text.length > 100) {
    text = text.substring(0, 97) + '...';
  }
  
  return text || null;
}

function generateSelector(element) {
  if (element.id) {
    return `#${CSS.escape(element.id)}`;
  }
  
  if (element.name) {
    return `${element.tagName.toLowerCase()}[name="${CSS.escape(element.name)}"]`;
  }
  
  if (element.className && typeof element.className === 'string') {
    const classes = element.className.trim().split(/\s+/).filter(c => c.length > 0);
    if (classes.length > 0) {
      const classSelector = classes.map(c => `.${CSS.escape(c)}`).join('');
      if (document.querySelectorAll(element.tagName + classSelector).length === 1) {
        return element.tagName.toLowerCase() + classSelector;
      }
    }
  }
  
  if (element.getAttribute('aria-label')) {
    const ariaLabel = element.getAttribute('aria-label');
    return `${element.tagName.toLowerCase()}[aria-label="${CSS.escape(ariaLabel)}"]`;
  }
  
  if (element.type) {
    return `${element.tagName.toLowerCase()}[type="${element.type}"]`;
  }
  
  const path = [];
  let current = element;
  while (current && current !== document.body) {
    let selector = current.tagName.toLowerCase();
    
    if (current.id) {
      path.unshift(`#${CSS.escape(current.id)}`);
      break;
    } else {
      let sibling = current;
      let nth = 1;
      while (sibling.previousElementSibling) {
        sibling = sibling.previousElementSibling;
        if (sibling.tagName === current.tagName) nth++;
      }
      if (nth > 1) {
        selector += `:nth-of-type(${nth})`;
      }
      path.unshift(selector);
    }
    
    current = current.parentElement;
  }
  
  return path.join(' > ');
}

function highlightElement(selector, color = '#00FF00', thickness = 4) {
  clearHighlight();
  
  try {
    const element = document.querySelector(selector);
    if (!element) {
      console.error('Element not found:', selector);
      return;
    }
    
    currentHighlightedElement = element;
    
    highlightOverlay = document.createElement('div');
    highlightOverlay.id = 'digital-helper-highlight';
    highlightOverlay.className = 'digital-helper-highlight';
    
    highlightOverlay.style.borderColor = color;
    highlightOverlay.style.borderWidth = `${thickness}px`;
    
    document.body.appendChild(highlightOverlay);
    
    updateHighlightPosition();
    
    window.addEventListener('scroll', updateHighlightPosition, true);
    window.addEventListener('resize', updateHighlightPosition);
    
    const observer = new MutationObserver(updateHighlightPosition);
    observer.observe(document.body, { 
      childList: true, 
      subtree: true, 
      attributes: true 
    });
    highlightOverlay._observer = observer;
    
  } catch (error) {
    console.error('Error highlighting element:', error);
  }
}

function updateHighlightPosition() {
  if (!highlightOverlay || !currentHighlightedElement) return;
  
  const rect = currentHighlightedElement.getBoundingClientRect();
  
  highlightOverlay.style.top = `${rect.top + window.scrollY}px`;
  highlightOverlay.style.left = `${rect.left + window.scrollX}px`;
  highlightOverlay.style.width = `${rect.width}px`;
  highlightOverlay.style.height = `${rect.height}px`;
}

function clearHighlight() {
  if (highlightOverlay) {
    if (highlightOverlay._observer) {
      highlightOverlay._observer.disconnect();
    }
    highlightOverlay.remove();
    highlightOverlay = null;
  }
  
  currentHighlightedElement = null;
  
  window.removeEventListener('scroll', updateHighlightPosition, true);
  window.removeEventListener('resize', updateHighlightPosition);
}

console.log('Digital Helper content script loaded');
