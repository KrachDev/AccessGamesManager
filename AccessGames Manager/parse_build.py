import subprocess

r = subprocess.run(['dotnet', 'build', 'AccessGames Manager.csproj', '--no-restore', '-v', 'q'], capture_output=True, text=True, cwd='.')
for l in (r.stdout + r.stderr).splitlines():
    if 'error' in l.lower():
        print(l)
