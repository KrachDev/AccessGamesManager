import sys

filepath = r'Views\MainWindow.axaml.cs'
with open(filepath, 'r', encoding='utf-8') as f:
    content = f.read()

# Update Play button to use localized string
old = '                    Content             = "▶  Play",'
new = '                    Content             = Localization.Get("PlayBtn"),'

if old not in content:
    print('ERROR: play btn string not found')
    sys.exit(1)

content = content.replace(old, new, 1)

with open(filepath, 'w', encoding='utf-8') as f:
    f.write(content)

print('Done')
