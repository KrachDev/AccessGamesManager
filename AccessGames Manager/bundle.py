import os
import re

html_path = r'C:\Users\Kracher\source\repos\AccessGames Manager\AccessGames Manager\AccessGamesWeb\public\catalogue.html'
css_path = r'C:\Users\Kracher\source\repos\AccessGames Manager\AccessGames Manager\AccessGamesWeb\public\css\style.css'
out_path = r'C:\Users\Kracher\source\repos\AccessGames Manager\AccessGames Manager\Assets\store.html'

with open(html_path, 'r', encoding='utf-8') as f:
    html = f.read()

with open(css_path, 'r', encoding='utf-8') as f:
    css = f.read()

# Inline CSS
html = html.replace('<link rel="stylesheet" href="/css/style.css">', f'<style>\n{css}\n</style>')

# Remove Navbar
html = re.sub(r'<nav class="navbar">.*?</nav>', '', html, flags=re.DOTALL)

# Remove Footer
html = re.sub(r'<footer class="footer">.*?</footer>', '', html, flags=re.DOTALL)

# Remove background particles if they exist
html = re.sub(r'<canvas id="particle-canvas".*?</canvas>', '', html, flags=re.DOTALL)

# Remove external JS
html = html.replace('<script src="/js/app.js"></script>', '')
html = html.replace('<script src="/js/nav.js"></script>', '')

old_script = """async function loadCatalogue() {
  try {
    allGames = await apiFetch('/api/games');
    buildCarousel(allGames);
    document.getElementById('catalogue-loading').style.opacity = '0';
    setTimeout(() => {
      document.getElementById('catalogue-loading').classList.add('hidden');
      document.getElementById('catalogue-grid').classList.remove('hidden');
      renderGames();
    }, 300);
  } catch(e) {
    document.getElementById('catalogue-loading').classList.add('hidden');
    document.getElementById('catalogue-empty').classList.remove('hidden');
  }
}"""

new_script = """
window.loadStoreData = function(jsonStr) {
  try {
    const rawOffers = JSON.parse(jsonStr);
    
    allGames = rawOffers.map(o => {
      let platStr = 'pc';
      if (o.Platform === 1) platStr = 'ps5';
      if (o.Platform === 2) platStr = 'xbox';
      
      return {
         gameId: o.Id,
         name: o.Title,
         imageUrl: o.CoverUrl,
         heroUrl: o.CoverUrl,
         platform: platStr,
         is_free: o.Price === 0,
         price: o.Price,
         has_activation: true,
         has_key: false,
         has_family_share: false,
         featured: o.IsHighlighted ? 1 : 0
      };
    });
    
    buildCarousel(allGames);
    document.getElementById('catalogue-loading').style.opacity = '0';
    setTimeout(() => {
      document.getElementById('catalogue-loading').classList.add('hidden');
      document.getElementById('catalogue-grid').classList.remove('hidden');
      renderGames();
    }, 300);
  } catch (e) {
    console.error("Parse error:", e);
    document.getElementById('catalogue-loading').classList.add('hidden');
    document.getElementById('catalogue-empty').classList.remove('hidden');
  }
};
"""

html = html.replace(old_script, new_script)
html = html.replace('loadCatalogue();', '')

# Replace links with webview.postMessage
html = html.replace("window.location='/game?id=${g.gameId}'", "window.chrome.webview.postMessage('offer:' + g.gameId)")

# Ensure body has no overflow issue 
custom_css = """
<style>
  html, body { overflow-y: auto !important; min-height: 100vh; background: transparent !important; } 
  body::before, body::after { display: none !important; }
  .page-wrapper { min-height: auto; }
  .page-content { padding: 10px 0 !important; }
  .container { padding: 0 16px; }
</style>
"""
html = html.replace('</head>', custom_css + '</head>')

with open(out_path, 'w', encoding='utf-8') as f:
    f.write(html)

print("bundled")
