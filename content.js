// content.js
let printDialogObserver;
let printButtonObserver;

function setupPrintDialogObservers() {
    // Create a mutation observer to detect when print dialog opens
    printDialogObserver = new MutationObserver((mutations) => {
        for (const mutation of mutations) {
            if (mutation.addedNodes) {
                for (const node of mutation.addedNodes) {
                    if (node.classList && node.classList.contains('dialog-container')) {
                        handlePrintDialogOpen();
                        return;
                    }
                }
            }
        }
    });
    
    // Start observing the document body
    printDialogObserver.observe(document.body, { childList: true, subtree: true });
}

function handlePrintDialogOpen() {
    // Find the print dialog
    const printDialog = document.querySelector('.dialog-container');
    if (!printDialog) return;
    
    // Create a new observer specifically for the print button
    printButtonObserver = new MutationObserver(() => {
        const printButton = printDialog.querySelector('.button-primary');
        const printerDropdown = printDialog.querySelector('select');
        
        if (printButton && printerDropdown) {
            // Check if our printer is selected
            const isCustomPrinter = printerDropdown.value.includes('Custom Home made Printer');
            
            // Modify the print button behavior
            printButton.addEventListener('click', (e) => {
                if (isCustomPrinter) {
                    e.preventDefault();
                    e.stopImmediatePropagation();
                    
                    // Get PDF content
                    const pdfUrl = window.location.href;
                    
                    // Send to native app
                    chrome.runtime.sendNativeMessage(
                        'com.yourcompany.customprinter',
                        {
                            action: "showPrintDialog",
                            content: {
                                type: "pdf",
                                url: pdfUrl
                            }
                        },
                        (response) => {
                            if (!response?.success) {
                                // Fallback to default printing
                                document.querySelector('.button-primary').click();
                            }
                        }
                    );
                }
            });
        }
    });
    
    // Observe the print dialog
    printButtonObserver.observe(printDialog, { childList: true, subtree: true });
}

// Initialize when page loads
if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', setupPrintDialogObservers);
} else {
    setupPrintDialogObservers();
}