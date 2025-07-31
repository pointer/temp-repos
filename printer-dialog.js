// printer-dialog.js
document.addEventListener('DOMContentLoaded', function() {
    // Add IDs to elements for easier access
    document.querySelector('.printer-select').id = 'printer-select';
    document.querySelector('.copies-input').id = 'copies-input';
    document.querySelector('.btn-print').id = 'btn-print';
    document.querySelector('.btn-cancel').id = 'btn-cancel';
    document.querySelector('.btn-help').id = 'btn-help';
    
    // Add values to radio buttons
    const actionRadios = document.querySelectorAll('input[name="action"]');
    actionRadios[0].value = 'Logo-Templates';
    actionRadios[1].value = 'Print Logo';
    actionRadios[2].value = 'HypoVereinsbank (deutsch)';
    actionRadios[3].value = 'Try Control';

    // Functionality for copy buttons
    document.getElementById('increase-copies').addEventListener('click', function() {
        const input = document.getElementById('copies-input');
        input.value = parseInt(input.value) + 1;
    });
    
    document.getElementById('decrease-copies').addEventListener('click', function() {
        const input = document.getElementById('copies-input');
        if (parseInt(input.value) > 1) {
            input.value = parseInt(input.value) - 1;
        }
    });
    
    // Radio button selection styling
    const options = document.querySelectorAll('.option');
    options.forEach(option => {
        option.addEventListener('click', function() {
            options.forEach(opt => opt.classList.remove('selected'));
            this.classList.add('selected');
            this.querySelector('input').checked = true;
        });
    });
    
    // Print button functionality
    document.getElementById('btn-print').addEventListener('click', function() {
        // Gather all settings
        const settings = {
            printer: document.getElementById('printer-select').value,
            copies: document.getElementById('copies-input').value,
            action: document.querySelector('input[name="action"]:checked').value,
            allPages: document.getElementById('all-pages').checked,
            timestamp: new Date().toISOString()
        };
        
        // Show printing status
        const originalText = this.textContent;
        this.textContent = 'Printing...';
        this.disabled = true;
        
        // Create status indicator
        const statusIndicator = document.querySelector('.status-indicator');
        statusIndicator.innerHTML = '<div class="status-dot" style="background:orange"></div> Processing...';
        
        // Simulate getting PDF content (in a real extension, this would be from Edge)
        getPdfContent()
            .then(pdfContent => {
                // Prepare data for native app
                const printData = {
                    action: "printPdf",
                    settings: settings,
                    pdfContent: pdfContent
                };
                
                // In a real Edge extension, you would use:
                // chrome.runtime.sendNativeMessage('com.yourcompany.customprinter', printData, handleResponse);
                
                // For demo, simulate native app response
                setTimeout(() => {
                    handleResponse({
                        success: true,
                        message: `PDF sent to ${settings.printer} successfully!`,
                        jobId: 'JOB-' + Date.now(),
                        settings: settings
                    });
                }, 2000);
            })
            .catch(error => {
                showError(error.message);
                this.textContent = originalText;
                this.disabled = false;
                statusIndicator.innerHTML = '<div class="status-dot" style="background:red"></div> Error';
            });
    });
    
    // Cancel button functionality
    document.getElementById('btn-cancel').addEventListener('click', function() {
        const dialog = document.querySelector('.printer-dialog');
        dialog.style.transform = 'translateY(-20px)';
        dialog.style.opacity = '0';
        
        setTimeout(() => {
            alert('Printing canceled');
            dialog.style.transform = 'translateY(0)';
            dialog.style.opacity = '1';
        }, 300);
    });
    
    // Help button functionality
    document.getElementById('btn-help').addEventListener('click', function() {
        alert('This dialog sends PDF content and print settings to your C# NativeMessagingHost.exe');
    });
    
    // Function to get PDF content (simulated)
    function getPdfContent() {
        return new Promise((resolve, reject) => {
            // In a real Edge extension, you would:
            // 1. Get the active tab
            // 2. Check if it's a PDF viewer
            // 3. Extract the PDF content
            
            // For this demo, we'll simulate with a sample base64 string
            try {
                const samplePdfBase64 = "JVBERi0xLjMKJcTl8uXrp/Og0MTGCjQgMCBvYmoKPDwvTGluZWFyaXplZCAxL0wgMTAwMDAvSCBbIDUwMCAxMDAwIF0vTiAxL1QgOTYwMC9WIDEwMDAwPj4KZW5kb2JqCjUgMCBvYmoKPDwvQ29udGVudHMgNiAwIFIvQ3JvcEJveFswIDAgNTk1IDg0Ml0vTWVkaWFCb3hbMCAwIDU5NSA4NDJdL1BhcmVudCA0IDAgUi9SZXNvdXJjZXM8PC9FeHRHU3RhdGU8PC9HUzAgNyAwIFI+Pi9Qcm9jU2V0Wy9QREYvVGV4dF0vWE9iamVjdDw8L0Zvcm0gOCAwIFI+Pj4vUm90YXRlIDAvVHlwZS9QYWdlPj4KZW5kb2JqCjYgMCBvYmoKPDwvRmlsdGVyL0ZsYXRlRGVjb2RlL0xlbmd0aCA0OD4+c3RyZWFtCnicU1VQcCjNKVZQcC7KzCsuTk1RMDSwNDYwMTZUMABiYwVDhQJDEz1jQwUjEwUjIxM9YwVDQwVjQwVzYwUjYwUzEwUzEwUjUwUTYwUzYwVjAwVDA0UjC0UAI2NCdQKZW5kc3RyZWFtCmVuZG9iago3IDAgb2JqCjw8L0NBIDEvY2EgMS9PUCBmYWxzZS9PUE0gMS9TQSB0cnVlL1NNYXNrL05vbmUvVHlwZS9FeHRHU3RhdGUvY2EgMS9vcCBmYWxzZT4+CmVuZG9iago4IDAgb2JqCjw8L0JCb3hbMCAwIDU5NSA4NDJdL0Zvcm1UeXBlIDEvTGVuZ3RoIDIyL01hdHJpeFsxIDAgMCAxIDAgMF0vUmVzb3VyY2VzPDwvUHJvY1NldFsvUERGXT4+L1N1YnR5cGUvRm9ybS9UeXBlL1hPYmplY3QvRmlsdGVyL0ZsYXRlRGVjb2RlPj5zdHJlYW0KeJwrVAhU0HXKdQ4u0Q8O0S9QcC7KzCsuTk1RMDSwNDYwMTZUMABiYwVDhQJDEz1jQwUjEwUjIxM9YwVDQwVjQwVzYwUjYwUzEwUzEwUjUwUTYwUzYwVjAwVDA0UjC0UAI2NCdQKZW5kc3RyZWFtCmVuZG9iagp0cmFpbGVyCjw8L1Jvb3QgMSAwIFIvU2l6ZSA5Pj4Kc3RhcnR4cmVmCjEwNzcKJSVFT0Y=";
                resolve(samplePdfBase64);
            } catch (error) {
                reject(new Error('Failed to load PDF content'));
            }
        });
    }
    
    // Handle response from native app
    function handleResponse(response) {
        const btnPrint = document.getElementById('btn-print');
        const statusIndicator = document.querySelector('.status-indicator');
        
        if (response && response.success) {
            // Create a success message
            const footer = document.querySelector('.footer');
            const receipt = document.createElement('div');
            receipt.className = 'print-receipt';
            receipt.innerHTML = `
                <div style="margin-top:10px; padding:10px; background:#e6f4ea; border-radius:6px;">
                    <p><strong>✅ Print Successful!</strong></p>
                    <p>Printer: ${response.settings.printer}</p>
                    <p>Copies: ${response.settings.copies}</p>
                    <p>Job ID: ${response.jobId}</p>
                </div>
            `;
            footer.parentNode.insertBefore(receipt, footer);
            
            // Update status indicator
            statusIndicator.innerHTML = '<div class="status-dot"></div> Ready';
        } else {
            // Show error
            showError(response?.message || 'Printing failed');
            statusIndicator.innerHTML = '<div class="status-dot" style="background:red"></div> Error';
        }
        
        // Reset print button
        btnPrint.textContent = 'Print';
        btnPrint.disabled = false;
    }
    
    // Show error message
    function showError(message) {
        const errorDiv = document.createElement('div');
        errorDiv.className = 'error-message';
        errorDiv.innerHTML = `<p style="color:red; padding:10px; background:#fce8e6; border-radius:4px;">❌ ${message}</p>`;
        
        const contentDiv = document.querySelector('.content');
        contentDiv.parentNode.insertBefore(errorDiv, contentDiv.nextSibling);
        
        // Remove error after 5 seconds
        setTimeout(() => {
            errorDiv.remove();
        }, 5000);
    }
    
    // Add custom styles for print receipt
    const style = document.createElement('style');
    style.textContent = `
        .print-receipt {
            padding: 0 25px 15px;
            animation: fadeIn 0.5s;
        }
        
        @keyframes fadeIn {
            from { opacity: 0; transform: translateY(-10px); }
            to { opacity: 1; transform: translateY(0); }
        }
        
        .error-message {
            padding: 0 25px 15px;
        }
    `;
    document.head.appendChild(style);
});