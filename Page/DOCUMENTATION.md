# AccessGames Manager — Website Documentation

> Complete record of the request, design decisions, and implementation details for the GitHub Pages landing page.

---

## Table of Contents

1. [Project Overview](#1-project-overview)
2. [The Original Request](#2-the-original-request)
3. [Source Material Gathered](#3-source-material-gathered)
4. [Design Decisions](#4-design-decisions)
5. [File Structure](#5-file-structure)
6. [Implementation Breakdown](#6-implementation-breakdown)
7. [Revisions Made](#7-revisions-made)
8. [Hosting on GitHub Pages](#8-hosting-on-github-pages)
9. [Maintenance Guide](#9-maintenance-guide)

---

## 1. Project Overview

**What it is:** A static single-page website built to serve as the public-facing download page for the AccessGames Manager desktop application.

**Hosted at:** GitHub Pages (via the app's repository or a dedicated repo)

**GitHub Repository:** [https://github.com/KrachDev/AccessGamesManager](https://github.com/KrachDev/AccessGamesManager)

**Target audience:** Steam users who want to manage multiple accounts, primarily Moroccan Arabic (Darija) speakers.

---

## 2. The Original Request

The user made three sequential requests that shaped the final output:

### Request 1 — Initial Build
> "I want a nice looking static website to download my app from GitHub. The website will be hosted on GitHub. I just want a webpage that will allow it to be easier on folks and a good explainer. The app project has everything you need on the app inner workings. It's a simple Steam account manager. Also the project folder contains some images from the app to show the app inner. I believe you can do great work."

**Key requirements extracted:**
- Static HTML (no backend, no build step)
- Hosted on GitHub Pages
- Explain the app clearly to non-technical users
- Use the existing app screenshots from `AppIMages/`
- Download button linking to GitHub releases
- Professional and visually appealing

### Request 2 — Corrections
> "Here's the repo: https://github.com/KrachDev/AccessGamesManager — also I want the website to be in Darija Arabic + don't mention the infinite loop fix, it's been removed."

**Changes required:**
- Fix the GitHub URL (was a placeholder guess, now confirmed)
- Translate entire site to Moroccan Darija Arabic
- Remove all references to the "Fix Infinite Loop" feature

### Request 3 — This Document
> "Make a md file in the directory explaining everything from the request to the implementation."

---

## 3. Source Material Gathered

Before writing a single line of HTML, the app's source code was read to understand the features, color scheme, and UI language.

### Files Inspected

| File | Purpose |
|------|---------|
| `AccessGames Manager/Views/MainWindow.axaml` | Full UI layout — tabs, buttons, colors, feature set |
| `AccessGamesManagerWeb/AppIMages/GamesTab.png` | Screenshot of the Games tab |
| `AccessGamesManagerWeb/AppIMages/AccountsTab.png` | Screenshot of the Accounts tab |
| `AccessGamesManagerWeb/AppIMages/SettingsTab.png` | Screenshot of the Settings tab |
| `.github/workflows/` | Checked for CI/CD and release pipeline info |

### Features Discovered from Source

Reading `MainWindow.axaml` revealed the following features to document on the site:

- **Games Tab** — WrapPanel grid of all Steam games with search and refresh
- **Accounts Tab** — WrapPanel of account cards with quick-switch
- **Store Tab** — Embedded store view
- **Settings Tab** — Contains:
  - Language selector (English, Français, الدارجة)
  - Firewall Control (Block Steam / Allow Steam)
  - Launch Mode (Auto / Force Online / Force Offline)
  - About section showing v2.0
- **Add Account button** — Clears auto-login, relaunches Steam to show login page
- ~~**Fix Infinite Loop button**~~ — *Removed from the app before site launch*
- **Network status indicator** — Live online/offline badge in the top bar

### Color Palette (Extracted from AXAML)

| Role | Hex | Used for |
|------|-----|---------|
| Background | `#0D0D14` | Main dark background |
| Surface | `#111120` | Cards, nav bar |
| Surface Alt | `#16162A` | Tab bar, window chrome |
| Border | `#1E1E30` | Dividers, card borders |
| Accent Purple | `#6C47FF` | Primary brand color, buttons, logo |
| Green | `#44FF88` | Online status, success states |
| Red | `#FF6060` | Danger buttons, offline/block |
| Muted Text | `#6666AA` | Secondary labels, counts |

---

## 4. Design Decisions

### Typography

| Font | Weight | Used for |
|------|--------|---------|
| **Cairo** | 400, 600, 700, 900 | All body text and UI labels — chosen for excellent Arabic/Darija rendering and clean Latin fallback |
| **Oxanium** | 700, 800 | Logo and nav only — keeps the tech/gaming brand identity |

Cairo was selected because it is purpose-built for Arabic script while remaining highly legible at small sizes. It supports the full Arabic glyph set needed for Darija.

### Direction

The entire page uses `dir="rtl"` on the `<html>` element. This causes:
- Text to align right by default
- Flex rows to reverse naturally
- The reading flow to match Arabic conventions

No JavaScript is needed for RTL — it is handled entirely by the browser via the HTML attribute.

### Visual Aesthetic

The site deliberately mirrors the app's own visual identity:
- Same dark background (`#0D0D14`)
- Same purple accent (`#6C47FF`)
- Same green for positive indicators (`#44FF88`)
- Grid-line background texture using CSS `background-image` gradients
- Radial purple glow at the top (matches the app's atmospheric feel)
- Fixed blurred orbs (`filter: blur(90px)`) for depth

This means a user who sees the website and then opens the app feels immediate visual continuity — the site *is* the app, aesthetically.

### Scroll Animations

All major sections use a `.fade-in` class with an `IntersectionObserver`. Elements start at `opacity: 0; transform: translateY(22px)` and transition to visible as they enter the viewport. Staggered delays (`.d1` through `.d4`) create a cascading reveal effect.

No animation library was used — pure CSS transitions triggered by a single 10-line JS observer.

### Screenshot Showcase

A tab switcher (Games / Accounts / Settings) was built with plain HTML buttons and JavaScript `classList` toggling — no framework. The screenshots sit inside a fake "window chrome" frame created entirely with CSS `::before` and `::after` pseudo-elements (the title bar and the `● ● ●` dots), so no extra HTML elements are needed.

---

## 5. File Structure

```
AccessGamesManagerWeb/
├── index.html          ← The entire website (single file)
├── DOCUMENTATION.md    ← This file
└── AppIMages/
    ├── GamesTab.png
    ├── AccountsTab.png
    └── SettingsTab.png
```

The images are referenced in the HTML as relative paths (`AppIMages/GamesTab.png`), so the folder structure must be preserved exactly when pushed to GitHub.

---

## 6. Implementation Breakdown

### HTML Structure (index.html)

The page is divided into 6 sections:

```
<nav>          Fixed navigation bar with logo + download CTA
<section.hero> Large headline, subtitle, action buttons, trust badges
<section.screenshots> Tab switcher with 3 app screenshots
<section.features>    6 feature cards in a responsive CSS grid
<section.how>         3-step "how to get started" with connecting line
<section.download>    Final CTA card + 4 system requirements tiles
<footer>       Logo, credit, links to Releases and Issues
```

### Key CSS Techniques

**Grid background texture:**
```css
background-image:
  linear-gradient(rgba(108,71,255,0.04) 1px, transparent 1px),
  linear-gradient(90deg, rgba(108,71,255,0.04) 1px, transparent 1px);
background-size: 48px 48px;
```
Creates a subtle purple grid that references the app's dark UI grid aesthetic.

**Gradient text (hero headline):**
```css
background: linear-gradient(135deg, #ffffff 0%, #aaaacc 100%);
-webkit-background-clip: text;
-webkit-text-fill-color: transparent;
background-clip: text;
```
The accented `<em>` uses the purple gradient version of the same technique.

**Frosted glass nav:**
```css
backdrop-filter: blur(16px);
-webkit-backdrop-filter: blur(16px);
background: rgba(13,13,20,0.88);
```
The nav becomes semi-transparent and blurs the content scrolling behind it.

**Feature card hover line:**
```css
.feature-card::before {
  content: ''; position: absolute;
  top: 0; left: 0; right: 0; height: 2px;
  background: transparent; transition: background 0.3s;
}
.feature-card:hover::before {
  background: linear-gradient(90deg, transparent, var(--purple), transparent);
}
```
A 2px gradient line sweeps in at the top of each card on hover.

**Screenshot window chrome (pure CSS):**
```css
.screenshot-frame::before {
  content: ''; display: block; height: 36px;
  background: var(--bg3); border-bottom: 1px solid var(--border);
}
.screenshot-frame::after {
  content: '● ● ●'; position: absolute; top: 10px; left: 14px;
  font-size: 10px; letter-spacing: 4px; color: var(--muted2);
}
```
Zero extra HTML elements — the title bar and traffic light dots are both pseudo-elements.

### JavaScript (inline, ~10 lines)

```javascript
// Tab switcher
function switchTab(name, el) {
  document.querySelectorAll('.screenshot-panel').forEach(p => p.classList.remove('active'));
  document.querySelectorAll('.tab').forEach(t => t.classList.remove('active'));
  document.getElementById('tab-' + name).classList.add('active');
  el.classList.add('active');
}

// Scroll reveal
const obs = new IntersectionObserver(entries => {
  entries.forEach(e => { if (e.isIntersecting) e.target.classList.add('visible'); });
}, { threshold: 0.1 });
document.querySelectorAll('.fade-in').forEach(el => obs.observe(el));
```

No jQuery, no frameworks, no build tools. The entire site runs as a plain `.html` file.

---

## 7. Revisions Made

### Revision 1 — GitHub URL Fix + Darija + Feature Removal

After the first version was written, three changes were applied in a full rewrite:

**GitHub URL:**
- Before: `https://github.com/KrachDev/AccessGames-Manager` (guessed placeholder)
- After: `https://github.com/KrachDev/AccessGamesManager` (confirmed by user)
- Changed in: nav CTA, hero download button, footer link, releases link, issues link, download section buttons (6 total occurrences)

**Language — Full Darija Translation:**
- `<html lang="ar-MA" dir="rtl">` added
- Font changed from `DM Sans` to `Cairo` (required for Arabic script)
- All visible text translated to Moroccan Darija Arabic
- RTL layout works without any additional CSS changes — the browser handles flex/grid direction reversal automatically

**Feature Removed — Fix Infinite Loop:**
- The first version had a feature card titled "Fix Infinite Loop" describing the `FixLoopBTN` found in the source AXAML
- This button was removed from the app before the website launch
- The feature card was deleted entirely and replaced with a "Store Tab" card to keep the grid at 6 items
- No other references to the feature existed

---

## 8. Hosting on GitHub Pages

### Steps to Deploy

1. Make sure your repository contains:
   ```
   index.html
   AppIMages/
     GamesTab.png
     AccountsTab.png
     SettingsTab.png
   ```

2. Push to GitHub:
   ```bash
   git add index.html AppIMages/ DOCUMENTATION.md
   git commit -m "Add GitHub Pages website"
   git push
   ```

3. In the repository on GitHub:
   - Go to **Settings → Pages**
   - Under **Source**, select your branch (usually `main`) and folder (`/ (root)`)
   - Click **Save**

4. GitHub will publish the site at:
   ```
   https://krachdev.github.io/AccessGamesManager/
   ```
   (May take 1–2 minutes to go live the first time.)

### No Build Step Required

The site is a single `.html` file with no dependencies, no `package.json`, no bundler. GitHub Pages serves it directly — nothing to configure beyond the steps above.

### Custom Domain (Optional)

To use a custom domain (e.g. `accessgames.io`):
1. Add a `CNAME` file to the repo root containing just your domain name
2. Configure your domain's DNS to point to GitHub Pages (see GitHub's documentation)

---

## 9. Maintenance Guide

### Updating the Download Link

The release URL is used in two buttons and the nav. All three follow this pattern:
```
https://github.com/KrachDev/AccessGamesManager/releases/latest
```
GitHub automatically redirects `/releases/latest` to the newest published release — you never need to update the URL when releasing a new version, as long as you publish it as a GitHub Release.

### Adding a New Screenshot

1. Place the image in `AppIMages/`
2. Add a new tab button in the `.tabs` div:
   ```html
   <button class="tab" onclick="switchTab('newtab', this)">🆕 الاسم</button>
   ```
3. Add the corresponding panel:
   ```html
   <div class="screenshot-panel" id="tab-newtab">
     <img src="AppIMages/NewTab.png" alt="وصف الصورة"/>
   </div>
   ```

### Adding or Removing a Feature Card

Feature cards live inside `.features-grid`. Each card follows this template:
```html
<div class="feature-card fade-in d1">
  <div class="feature-icon icon-purple">🔧</div>
  <h3>عنوان الخاصية</h3>
  <p>وصف مختصر للخاصية.</p>
</div>
```
Icon color classes available: `icon-purple`, `icon-green`, `icon-red`, `icon-blue`.

### Changing the Version Badge

In the hero section, find:
```html
<div class="hero-badge fade-in">
  <span class="dot"></span>
  الإصدار v2.0 — مفتوح المصدر ومجاني
</div>
```
Update `v2.0` to the new version number.

---

*Last updated: April 2026 — Built by Claude (Anthropic) on request of KrachDev*
