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

