The story, of the Edge Print Extension, is that when the user opens PDF file using Edge browser, then when the user needs to print the pdf file, he clicks on the Printer icon on the toolbar in green in the image (2nd image) of the toolbar. Edge then opens the Print dialog like in the image of the Print dialog box(1st image) . The user choses the Custom Home made from the Printer drop down widget and then when the Custom Home made Printer Name is chosen and displayed the user clicks on the Print blue button on the bottom left corner of the open Printer dialog box. The goal is to intercept the user Click on that Print button in the bottom left of the opened printer dialog, and call the Native Messaging Host to display the Custom Home Printing dialog box.

The print dialog in Edge is a system dialog, and extensions cannot directly interact with it.
Approach
1. **Override the Print Button in Edge's Toolbar**: We can create an extension that adds a custom print button to the Edge toolbar. When clicked, it bypasses the built-in print dialog and directly communicates with the native host to show the custom dialog.
2. **Intercept the Print Command (Ctrl+P)**: As we previously discussed, but this time we are not limited to the system print dialog.

The user is expected to use the printer icon in the Edge toolbar to open the print dialog. Then, in that dialog, after selecting the custom printer, clicking the print button should trigger our custom dialog.
Unfortunately, this is not possible because:
- The print dialog is a system-level dialog, and extensions cannot detect events inside it.
- Extensions cannot detect which printer is selected in the system print dialog.

  We can design the flow as follows:
1. **Replace the built-in print button with our own**: We can inject a custom button next to the built-in print button. When the user clicks our button, we bypass the built-in print dialog and show our custom dialog via the native host.
2. **Steps**:
   - Create a browser extension that adds a custom print button in the Edge toolbar (using the `action` property in the manifest to set a popup or trigger a background script).
   - When the custom print button is clicked, the extension gathers the current page content (or PDF document) and sends it to the native host via Native Messaging.
   - The native host (C# application) then displays the custom print dialog.

In Manifest V3, you cannot use `edge://` in `matches`. So we have to use an alternative approach.
### Alternative for PDF
We can use the `webRequest` API to intercept the PDF request and then get the PDF data. However, this is complex and requires additional permissions.
Given the complexity, and since the requirement is to print when the user selects a custom printer and clicks print in the system dialog, which we cannot intercept, we must provide an alternative user flow: **Use our custom toolbar button for printing with the custom printer**.
This solution bypasses the built-in print dialog entirely. When the user wants to use the custom printer, they click our extension button instead of the built-in print button.
### Conclusion
We cannot intercept the built-in print dialog's print button. Therefore, we change the user flow: We provide a separate button in the toolbar for printing with the custom printer. This button will trigger the native host and show the custom dialog.
The user must be trained to use our extension button for printing to the custom printer, and the built-in print button for other printers.   

Solution: Intercepting Edge Print Dialog for Custom Printer
We'll create a solution that intercepts the print action when your custom printer is selected and triggers your custom dialog instead. This requires a combination of Edge extension techniques and native messaging.

Implementation Overview
Create a content script that monitors the print dialog

Detect when your custom printer is selected

Intercept the print button click

Communicate with your native host to show the custom dialog


NativeMessagingHost.exe (bridge between browser & C#).
Why is NativeMessagingHost.exe required?

Browsers cannot directly call C# applications due to security restrictions.

The host acts as a middleman, receiving JSON from the browser and invoking your Helper DLL.

✅ How does the browser communicate with it?

Uses stdin/stdout pipes (Chrome/Edge Native Messaging API).

Sends a 4-byte length header followed by JSON.

✅ What if my dialog doesn’t appear?

Manifest file (tells Chrome/Edge where to find the host).

Extension popup (triggers printing).

Option B: Manual Registration
Save the JSON manifest to:

Chrome:
%LOCALAPPDATA%\Google\Chrome\NativeMessagingHosts\com.yourcompany.customprinter.json

Edge:
%LOCALAPPDATA%\Microsoft\Edge\User Data\NativeMessagingHosts\com.yourcompany.customprinter.json

Replace YOUREXTENSIONIDHERE with your actual extension ID (get it from chrome://extensions).

Decode the base64 PDF content

Apply the print settings

Send the document to the selected printer

Return a success response with job details

 data structure sent to your C# application

 Key Features of this Implementation:
PDF Preview and Upload Functionality:

Embedded PDF viewer with iframe

Load sample PDF option

Upload your own PDF files

Responsive layout for different screen sizes

Complete Printer Settings:

Printer selection dropdown

Copies control with increment/decrement buttons

Action options with radio button selection

Page settings (All Pages, Auto Select)

Additional options (Color Printing, Double-Sided)

Native Messaging Integration:

"Send to Native Printer" button collects all settings

Simulates PDF content retrieval (base64 encoded)

Formats data for NativeMessagingHost.exe

Handles success/error responses

User Experience Enhancements:

Status messages with success/error states

Loading animation during processing

Print receipt with job details

Help and cancel functionality

Responsive design for all devices


  <script>
    // Generate nonce before any scripts run
    const nonce = generateNonce();
    // Apply it to the CSP meta tag (see Step 3)
    document.addEventListener('DOMContentLoaded', () => {
      const meta = document.createElement('meta');
      meta.httpEquiv = "Content-Security-Policy";
      meta.content = `script-src 'nonce-${nonce}' 'self'; object-src 'none';`;
      document.head.appendChild(meta);
    });
  </script>
  
  <!-- Inline script using the nonce -->
  <script nonce="${nonce}">
    console.log("This inline script is allowed!");
  </script>
