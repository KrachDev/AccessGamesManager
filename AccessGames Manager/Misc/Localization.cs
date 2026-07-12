using System.Collections.Generic;

namespace AccessGamesManager.Misc
{
    public enum AppLanguage { English, French, Darija, Arabic }

    public static class Localization
    {
        public static AppLanguage Current { get; private set; } = AppLanguage.English;

        public static void SetLanguage(AppLanguage lang) => Current = lang;

        private static readonly Dictionary<string, Dictionary<AppLanguage, string>> _strings = new()
        {
            // ── Top bar ───────────────────────────────────────────────────────
            ["AddNewAccount"]        = new() { [AppLanguage.English] = "＋  Add New Account",        [AppLanguage.French] = "＋  Ajouter un compte",           [AppLanguage.Darija] = "＋  زيد حساب جديد", [AppLanguage.Arabic] = "＋  إضافة حساب جديد" },

            // ── Nav ───────────────────────────────────────────────────────────
            ["NavGames"]             = new() { [AppLanguage.English] = "🎮  Games",                   [AppLanguage.French] = "🎮  Jeux",                         [AppLanguage.Darija] = "🎮  الألعاب", [AppLanguage.Arabic] = "🎮  الألعاب" },
            ["NavStore"]             = new() { [AppLanguage.English] = "🛒  Store",                   [AppLanguage.French] = "🛒  Boutique",                     [AppLanguage.Darija] = "🛒  المتجر", [AppLanguage.Arabic] = "🛒  المتجر" },
            ["NavAccounts"]          = new() { [AppLanguage.English] = "👤  Accounts",                [AppLanguage.French] = "👤  Comptes",                      [AppLanguage.Darija] = "👤  الحسابات", [AppLanguage.Arabic] = "👤  الحسابات" },
            ["NavLaunchers"]         = new() { [AppLanguage.English] = "🚀  Launchers",               [AppLanguage.French] = "🚀  Lanceurs",                     [AppLanguage.Darija] = "🚀  اللانشرات", [AppLanguage.Arabic] = "🚀  المشغلات" },
            ["NavSettings"]          = new() { [AppLanguage.English] = "⚙  Settings",                [AppLanguage.French] = "⚙  Paramètres",                   [AppLanguage.Darija] = "⚙  الإعدادات", [AppLanguage.Arabic] = "⚙  الإعدادات" },

            // ── Games page ────────────────────────────────────────────────────
            ["Library"]              = new() { [AppLanguage.English] = "Library",                     [AppLanguage.French] = "Bibliothèque",                     [AppLanguage.Darija] = "المكتبة", [AppLanguage.Arabic] = "المكتبة" },
            ["GamesCount"]           = new() { [AppLanguage.English] = "games",                       [AppLanguage.French] = "jeux",                             [AppLanguage.Darija] = "لعبة", [AppLanguage.Arabic] = "ألعاب" },
            ["RefreshGames"]         = new() { [AppLanguage.English] = "↺  Refresh",                  [AppLanguage.French] = "↺  Actualiser",                    [AppLanguage.Darija] = "↺  تحديث", [AppLanguage.Arabic] = "↺  تحديث" },
            ["GameTooltipOwner"]     = new() { [AppLanguage.English] = "Owner",                       [AppLanguage.French] = "Propriétaire",                     [AppLanguage.Darija] = "صاحب الحساب", [AppLanguage.Arabic] = "المالك" },
            ["GameTooltipLaunch"]    = new() { [AppLanguage.English] = "Click to launch",             [AppLanguage.French] = "Cliquer pour lancer",              [AppLanguage.Darija] = "كليك باش تلعب", [AppLanguage.Arabic] = "انقر للتشغيل" },
            ["UnknownOwner"]         = new() { [AppLanguage.English] = "Unknown",                     [AppLanguage.French] = "Inconnu",                          [AppLanguage.Darija] = "مجهول", [AppLanguage.Arabic] = "غير معروف" },
            ["PlayBtn"]              = new() { [AppLanguage.English] = "▶  Play",                      [AppLanguage.French] = "▶  Jouer",                         [AppLanguage.Darija] = "▶  العب", [AppLanguage.Arabic] = "▶  تشغيل" },
            ["ResetPlaytime"]        = new() { [AppLanguage.English] = "⏱  Reset Playtime",            [AppLanguage.French] = "⏱  Réinit. le temps de jeu",      [AppLanguage.Darija] = "⏱  مسح وقت اللعب", [AppLanguage.Arabic] = "⏱  إعادة تعيين وقت اللعب" },
            ["ResetAchievements"]    = new() { [AppLanguage.English] = "🏆  Reset Achievements",        [AppLanguage.French] = "🏆  Réinit. les succès",           [AppLanguage.Darija] = "🏆  مسح الإنجازات", [AppLanguage.Arabic] = "🏆  إعادة تعيين الإنجازات" },
            ["ChangeAccount"]        = new() { [AppLanguage.English] = "Launch via account:",           [AppLanguage.French] = "Lancer avec le compte :",          [AppLanguage.Darija] = "شغّل بالحساب:", [AppLanguage.Arabic] = "تشغيل بواسطة حساب:" },
            ["DefaultAccount"]       = new() { [AppLanguage.English] = "Default: {0}",                 [AppLanguage.French] = "Par défaut : {0}",                  [AppLanguage.Darija] = "الافتراضي: {0}", [AppLanguage.Arabic] = "الافتراضي: {0}" },

            // ── Accounts page ─────────────────────────────────────────────────
            ["Accounts"]             = new() { [AppLanguage.English] = "Accounts",                    [AppLanguage.French] = "Comptes",                          [AppLanguage.Darija] = "الحسابات", [AppLanguage.Arabic] = "الحسابات" },
            ["AccountsCount"]        = new() { [AppLanguage.English] = "accounts",                    [AppLanguage.French] = "comptes",                          [AppLanguage.Darija] = "حساب", [AppLanguage.Arabic] = "حسابات" },
            ["RefreshAccounts"]      = new() { [AppLanguage.English] = "↺  Refresh",                  [AppLanguage.French] = "↺  Actualiser",                    [AppLanguage.Darija] = "↺  تحديث", [AppLanguage.Arabic] = "↺  تحديث" },

            // Account card role badges
            ["RolePersonal"]         = new() { [AppLanguage.English] = "⭐ Personal",                 [AppLanguage.French] = "⭐ Personnel",                     [AppLanguage.Darija] = "⭐ شخصي", [AppLanguage.Arabic] = "⭐ شخصي" },
            ["RoleAccess"]           = new() { [AppLanguage.English] = "🎮 Access",                   [AppLanguage.French] = "🎮 Accès",                         [AppLanguage.Darija] = "🎮 أكسس", [AppLanguage.Arabic] = "🎮 أكسس" },

            // Account card switch button
            ["SwitchBtnOnline"]      = new() { [AppLanguage.English] = "Login Normally",              [AppLanguage.French] = "Connexion normale",                [AppLanguage.Darija] = "دخل بشكل عادي", [AppLanguage.Arabic] = "تسجيل الدخول العادي" },
            ["SwitchBtnOffline"]     = new() { [AppLanguage.English] = "Switch (Offline)",            [AppLanguage.French] = "Changer (Hors ligne)",             [AppLanguage.Darija] = "بدل (أوفلاين)", [AppLanguage.Arabic] = "تبديل (أوفلاين)" },
            ["SwitchTooltip"]        = new() { [AppLanguage.English] = "Switch to",                   [AppLanguage.French] = "Changer vers",                     [AppLanguage.Darija] = "بدل إلى", [AppLanguage.Arabic] = "تبديل إلى" },

            // ── Settings page ─────────────────────────────────────────────────
            ["SettingsTitle"]        = new() { [AppLanguage.English] = "Settings",                    [AppLanguage.French] = "Paramètres",                       [AppLanguage.Darija] = "الإعدادات", [AppLanguage.Arabic] = "الإعدادات" },
            ["LanguageSection"]      = new() { [AppLanguage.English] = "LANGUAGE",                    [AppLanguage.French] = "LANGUE",                           [AppLanguage.Darija] = "اللغة", [AppLanguage.Arabic] = "اللغة" },
            ["LanguageLabel"]        = new() { [AppLanguage.English] = "Language",                    [AppLanguage.French] = "Langue",                           [AppLanguage.Darija] = "اللغة", [AppLanguage.Arabic] = "اللغة" },
            ["NetworkSection"]       = new() { [AppLanguage.English] = "NETWORK",                     [AppLanguage.French] = "RÉSEAU",                           [AppLanguage.Darija] = "الشبكة", [AppLanguage.Arabic] = "الشبكة" },
            ["FirewallControl"]      = new() { [AppLanguage.English] = "Firewall Control",            [AppLanguage.French] = "Contrôle du pare-feu",             [AppLanguage.Darija] = "التحكم في الجدار الناري", [AppLanguage.Arabic] = "التحكم في الجدار الناري" },
            ["FirewallDescOn"]       = new() { [AppLanguage.English] = "Steam can currently access the internet.",         [AppLanguage.French] = "Steam peut actuellement accéder à Internet.",         [AppLanguage.Darija] = "ستيم قادر يوصل للإنترنت دابا.", [AppLanguage.Arabic] = "ستيم قادر على الوصول إلى الإنترنت حالياً." },
            ["FirewallDescOff"]      = new() { [AppLanguage.English] = "Steam is blocked — it cannot reach the internet.", [AppLanguage.French] = "Steam est bloqué — il ne peut pas accéder à Internet.", [AppLanguage.Darija] = "ستيم محجوب — ما قادرش يوصل للإنترنت.", [AppLanguage.Arabic] = "ستيم محجوب — لا يمكنه الوصول إلى الإنترنت." },
            ["BlockSteam"]           = new() { [AppLanguage.English] = "🔒  Block Steam",             [AppLanguage.French] = "🔒  Bloquer Steam",                [AppLanguage.Darija] = "🔒  حجب ستيم", [AppLanguage.Arabic] = "🔒  حجب ستيم" },
            ["AllowSteam"]           = new() { [AppLanguage.English] = "🔓  Allow Steam",             [AppLanguage.French] = "🔓  Autoriser Steam",              [AppLanguage.Darija] = "🔓  سمح لستيم", [AppLanguage.Arabic] = "🔓  السماح لستيم" },
            ["GrowlBlocked"]         = new() { [AppLanguage.English] = "Steam blocked — firewall rule applied.",           [AppLanguage.French] = "Steam bloqué — règle de pare-feu appliquée.",        [AppLanguage.Darija] = "ستيم تحجب — القاعدة ديال الجدار الناري تطبقات.", [AppLanguage.Arabic] = "تم حجب ستيم — تم تطبيق قاعدة الجدار الناري." },
            ["GrowlUnblocked"]       = new() { [AppLanguage.English] = "Steam unblocked — firewall rule removed.",        [AppLanguage.French] = "Steam débloqué — règle de pare-feu supprimée.",      [AppLanguage.Darija] = "ستيم تحل — القاعدة ديال الجدار الناري تمسحات.", [AppLanguage.Arabic] = "تم السماح لستيم — تم إزالة قاعدة الجدار الناري." },
            ["LaunchModeSection"]    = new() { [AppLanguage.English] = "LAUNCH MODE",                 [AppLanguage.French] = "MODE DE LANCEMENT",               [AppLanguage.Darija] = "وضع التشغيل", [AppLanguage.Arabic] = "وضع التشغيل" },
            ["ForceLaunchMode"]      = new() { [AppLanguage.English] = "Force Launch Mode",           [AppLanguage.French] = "Mode de lancement forcé",          [AppLanguage.Darija] = "إجبار وضع التشغيل", [AppLanguage.Arabic] = "إجبار وضع التشغيل" },
            ["ForceLaunchDesc"]      = new() { [AppLanguage.English] = "Override the per-account role logic. Auto uses the account\u2019s role (Personal = Online, Access = Offline).", [AppLanguage.French] = "Remplacer la logique de r\u00f4le par compte. Auto utilise le r\u00f4le du compte (Personnel = En ligne, Acc\u00e8s = Hors ligne).", [AppLanguage.Darija] = "بدل منطق الدور ديال الحساب. أوتو كيستعمل دور الحساب (شخصي = أونلاين، أكسس = أوفلاين).", [AppLanguage.Arabic] = "تجاوز منطق الدور لكل حساب. التلقائي يستخدم دور الحساب (شخصي = أونلاين، أكسس = أوفلاين)." },
            ["LaunchAuto"]           = new() { [AppLanguage.English] = "⚡  Auto  (use account role)", [AppLanguage.French] = "⚡  Auto  (utiliser le rôle du compte)", [AppLanguage.Darija] = "⚡  تلقائي  (حسب دور الحساب)", [AppLanguage.Arabic] = "⚡  تلقائي  (حسب دور الحساب)" },
            ["LaunchForceOnline"]    = new() { [AppLanguage.English] = "🌐  Force Online  (always online)",   [AppLanguage.French] = "🌐  Forcer En ligne  (toujours en ligne)",    [AppLanguage.Darija] = "🌐  أونلاين دايما", [AppLanguage.Arabic] = "🌐  إجبار أونلاين  (دائماً أونلاين)" },
            ["LaunchForceOffline"]   = new() { [AppLanguage.English] = "🔒  Force Offline  (always offline)", [AppLanguage.French] = "🔒  Forcer Hors ligne  (toujours hors ligne)", [AppLanguage.Darija] = "🔒  أوفلاين دايما", [AppLanguage.Arabic] = "🔒  إجبار أوفلاين  (دائماً أوفلاين)" },
            ["AboutSection"]         = new() { [AppLanguage.English] = "ABOUT",                       [AppLanguage.French] = "À PROPOS",                         [AppLanguage.Darija] = "علينا", [AppLanguage.Arabic] = "حول التطبيق" },
            ["AboutDesc"]            = new() { [AppLanguage.English] = "Manage your game library and accounts. Switch accounts with automatic firewall control to keep your session active.", [AppLanguage.French] = "Gérez votre bibliothèque de jeux et vos comptes. Changez de compte avec contrôle automatique du pare-feu.", [AppLanguage.Darija] = "دير في مكتبة الألعاب والحسابات ديالك. بدل الحسابات مع تحكم تلقائي في الجدار الناري.", [AppLanguage.Arabic] = "إدارة مكتبة الألعاب والحسابات الخاصة بك. التبديل بين الحسابات مع التحكم التلقائي في الجدار الناري لإبقاء جلستك نشطة." },

            // ── Status bar ────────────────────────────────────────────────────
            ["StatusReady"]          = new() { [AppLanguage.English] = "Ready",                       [AppLanguage.French] = "Prêt",                             [AppLanguage.Darija] = "جاهز", [AppLanguage.Arabic] = "جاهز" },
            ["StatusLoadingGames"]   = new() { [AppLanguage.English] = "Loading games…",              [AppLanguage.French] = "Chargement des jeux…",             [AppLanguage.Darija] = "كيتحملو الألعاب…", [AppLanguage.Arabic] = "جاري تحميل الألعاب..." },
            ["StatusLoadingAccs"]    = new() { [AppLanguage.English] = "Loading accounts…",           [AppLanguage.French] = "Chargement des comptes…",          [AppLanguage.Darija] = "كيتحملو الحسابات…", [AppLanguage.Arabic] = "جاري تحميل الحسابات..." },
            ["StatusLoadedGames"]    = new() { [AppLanguage.English] = "Loaded {0} games",            [AppLanguage.French] = "{0} jeux chargés",                 [AppLanguage.Darija] = "تحملو {0} لعبة", [AppLanguage.Arabic] = "تم تحميل {0} ألعاب" },
            ["StatusLoadedAccs"]     = new() { [AppLanguage.English] = "Loaded {0} accounts",         [AppLanguage.French] = "{0} comptes chargés",              [AppLanguage.Darija] = "تحملو {0} حساب", [AppLanguage.Arabic] = "تم تحميل {0} حسابات" },
            ["StatusBlocked"]        = new() { [AppLanguage.English] = "🔒 Steam network blocked",    [AppLanguage.French] = "🔒 Réseau Steam bloqué",           [AppLanguage.Darija] = "🔒 شبكة ستيم محجوبة", [AppLanguage.Arabic] = "🔒 تم حظر شبكة ستيم" },
            ["StatusOpen"]           = new() { [AppLanguage.English] = "🔓 Steam network open",       [AppLanguage.French] = "🔓 Réseau Steam ouvert",           [AppLanguage.Darija] = "🔓 شبكة ستيم مفتوحة", [AppLanguage.Arabic] = "🔓 شبكة ستيم مفتوحة" },
            ["StatusSwitching"]      = new() { [AppLanguage.English] = "Switching to {0}…",           [AppLanguage.French] = "Changement vers {0}…",             [AppLanguage.Darija] = "كيتبدل إلى {0}…", [AppLanguage.Arabic] = "جاري التبديل إلى {0}..." },
            ["StatusLaunching"]      = new() { [AppLanguage.English] = "🚀 Launching {0} as {1} ({2})…", [AppLanguage.French] = "🚀 Lancement de {0} en tant que {1} ({2})…", [AppLanguage.Darija] = "🚀 كيتلعب {0} بحساب {1} ({2})…", [AppLanguage.Arabic] = "🚀 جاري تشغيل {0} كـ {1} ({2})..." },
            ["StatusSteamRestart"]   = new() { [AppLanguage.English] = "Steam restarting — login page will appear", [AppLanguage.French] = "Steam redémarre — la page de connexion va apparaître", [AppLanguage.Darija] = "ستيم كيعاود — صفحة الدخول غادي تظهر", [AppLanguage.Arabic] = "إعادة تشغيل ستيم — ستظهر صفحة تسجيل الدخول" },
            ["StatusLaunchModeAuto"] = new() { [AppLanguage.English] = "Launch mode: Auto (uses account role)",    [AppLanguage.French] = "Mode de lancement : Auto (rôle du compte)",          [AppLanguage.Darija] = "وضع التشغيل: تلقائي (حسب دور الحساب)", [AppLanguage.Arabic] = "وضع التشغيل: تلقائي (حسب دور الحساب)" },
            ["StatusLaunchOnline"]   = new() { [AppLanguage.English] = "Launch mode: Force Online",               [AppLanguage.French] = "Mode de lancement : Forcer En ligne",                [AppLanguage.Darija] = "وضع التشغيل: أونلاين دايما", [AppLanguage.Arabic] = "وضع التشغيل: إجبار أونلاين" },
            ["StatusLaunchOffline"]  = new() { [AppLanguage.English] = "Launch mode: Force Offline",              [AppLanguage.French] = "Mode de lancement : Forcer Hors ligne",              [AppLanguage.Darija] = "وضع التشغيل: أوفلاين دايما", [AppLanguage.Arabic] = "وضع التشغيل: إجبار أوفلاين" },
            ["NoAccountToLaunch"]    = new() { [AppLanguage.English] = "No account found to launch {0}.",         [AppLanguage.French] = "Aucun compte trouvé pour lancer {0}.",               [AppLanguage.Darija] = "ما لقيناش حساب باش نلعبو {0}.", [AppLanguage.Arabic] = "لم يتم العثور على حساب لتشغيل {0}." },

            // ── Already signed in dialog ────────────────────────────────────────────────────────
            ["AlreadySignedInTitle"] = new() { [AppLanguage.English] = "Account already signed in",         [AppLanguage.French] = "Compte déjà connecté",              [AppLanguage.Darija] = "الحساب دخل بالفعل", [AppLanguage.Arabic] = "الحساب مسجل الدخول بالفعل" },
            ["AlreadySignedInMsg"]   = new() { [AppLanguage.English] = "{0} is already the active Steam account. What do you want to do?", [AppLanguage.French] = "{0} est déjà le compte Steam actif. Que voulez-vous faire ?", [AppLanguage.Darija] = "{0} دخلاتي بالفعل. شنو بغيتي ديري ؟", [AppLanguage.Arabic] = "{0} هو حساب ستيم النشط بالفعل. ماذا تريد أن تفعل؟" },
            ["AlreadySignedInReboot"]= new() { [AppLanguage.English] = "Reboot Steam",                      [AppLanguage.French] = "Redémarrer Steam",                [AppLanguage.Darija] = "عاود تشغيل ستيم", [AppLanguage.Arabic] = "إعادة تشغيل ستيم" },
            ["AlreadySignedInLaunch"]= new() { [AppLanguage.English] = "Just launch the game",              [AppLanguage.French] = "Lancer le jeu seulement",         [AppLanguage.Darija] = "لانساب غير تشغيل ستيم", [AppLanguage.Arabic] = "تشغيل اللعبة فقط" },
            ["AlreadySignedInCancel"]= new() { [AppLanguage.English] = "Cancel",                             [AppLanguage.French] = "Annuler",                          [AppLanguage.Darija] = "إلغاء", [AppLanguage.Arabic] = "إلغاء" },

            // ── Network pill ──────────────────────────────────────────────────
            ["Online"]               = new() { [AppLanguage.English] = "Online",                      [AppLanguage.French] = "En ligne",                         [AppLanguage.Darija] = "أونلاين", [AppLanguage.Arabic] = "أونلاين" },
            ["Offline"]              = new() { [AppLanguage.English] = "Offline",                     [AppLanguage.French] = "Hors ligne",                       [AppLanguage.Darija] = "أوفلاين", [AppLanguage.Arabic] = "أوفلاين" },
            ["ONLINE"]               = new() { [AppLanguage.English] = "ONLINE",                      [AppLanguage.French] = "EN LIGNE",                         [AppLanguage.Darija] = "أونلاين", [AppLanguage.Arabic] = "متصل" },
            ["OFFLINE"]              = new() { [AppLanguage.English] = "OFFLINE",                     [AppLanguage.French] = "HORS LIGNE",                       [AppLanguage.Darija] = "أوفلاين", [AppLanguage.Arabic] = "غير متصل" },

            // ── App version footer ────────────────────────────────────────────
            ["AppVersion"]           = new() { [AppLanguage.English] = "AccessGames Manager v2.5.3",    [AppLanguage.French] = "AccessGames Manager v2.5.3",         [AppLanguage.Darija] = "AccessGames Manager v2.5.3", [AppLanguage.Arabic] = "AccessGames Manager v2.5.3" },

            // ── Launchers page ────────────────────────────────────────────────
            ["LaunchersTitle"]       = new() { [AppLanguage.English] = "Game Launchers",              [AppLanguage.French] = "Lanceurs de jeux",                 [AppLanguage.Darija] = "اللانشرات ديال الألعاب", [AppLanguage.Arabic] = "مشغلات الألعاب" },
            ["FirewallControlHeader"] = new() { [AppLanguage.English] = "FIREWALL CONTROL",            [AppLanguage.French] = "CONTRÔLE DU PARE-FEU",             [AppLanguage.Darija] = "التحكم في الجدار الناري", [AppLanguage.Arabic] = "التحكم في الجدار الناري" },
            ["SettingsHeader"]       = new() { [AppLanguage.English] = "SETTINGS",                    [AppLanguage.French] = "PARAMÈTRES",                       [AppLanguage.Darija] = "الإعدادات", [AppLanguage.Arabic] = "الإعدادات" },
            ["PathsConfigTitle"]     = new() { [AppLanguage.English] = "Paths Configuration",          [AppLanguage.French] = "Configuration des chemins",         [AppLanguage.Darija] = "تكوين المسارات", [AppLanguage.Arabic] = "تكوين المسارات" },
            ["LauncherNotDetected"]  = new() { [AppLanguage.English] = "Launcher not detected.",         [AppLanguage.French] = "Lanceur non détecté.",             [AppLanguage.Darija] = "اللانشر ما كاينش.", [AppLanguage.Arabic] = "لم يتم اكتشاف المشغل." },
            ["LauncherFoundDefault"] = new() { [AppLanguage.English] = "Found at default location.",        [AppLanguage.French] = "Trouvé à l'emplacement par défaut.", [AppLanguage.Darija] = "تلقى في البلاصة الافتراضية.", [AppLanguage.Arabic] = "تم العثور عليه في الموقع الافتراضي." },
            ["BadgeBlocked"]         = new() { [AppLanguage.English] = "🔒 BLOCKED",                  [AppLanguage.French] = "🔒 BLOQUÉ",                        [AppLanguage.Darija] = "🔒 محجوب", [AppLanguage.Arabic] = "🔒 محجوب" },
            ["BadgeAllowed"]         = new() { [AppLanguage.English] = "✓ ALLOWED",                  [AppLanguage.French] = "✓ AUTORISÉ",                      [AppLanguage.Darija] = "✓ مسموح", [AppLanguage.Arabic] = "✓ مسموح" },
            ["BtnBlock"]             = new() { [AppLanguage.English] = "🔒  Block",                   [AppLanguage.French] = "🔒  Bloquer",                      [AppLanguage.Darija] = "🔒  حجب", [AppLanguage.Arabic] = "🔒  حجب" },
            ["BtnAllow"]             = new() { [AppLanguage.English] = "🔓  Allow",                   [AppLanguage.French] = "🔓  Autoriser",                    [AppLanguage.Darija] = "🔓  سمح", [AppLanguage.Arabic] = "🔓  سماح" },
            
            // Steps
            ["UbiStep1"]             = new() { [AppLanguage.English] = "Step 1: Disable active network adapters & turn on Airplane Mode", [AppLanguage.French] = "Étape 1: Désactiver les cartes réseau actives et activer le mode avion", [AppLanguage.Darija] = "الخطوة 1: طفي الكارط ريزو لي خدامة وشعل وضع الطيران", [AppLanguage.Arabic] = "الخطوة 1: تعطيل محولات الشبكة النشطة وتفعيل وضع الطيران" },
            ["UbiStep2Block"]        = new() { [AppLanguage.English] = "Step 2: Add outbound/inbound firewall block rules for Ubisoft Connect", [AppLanguage.French] = "Étape 2: Ajouter des règles de blocage de pare-feu pour Ubisoft Connect", [AppLanguage.Darija] = "الخطوة 2: زيد قواعد الحجب في الجدار الناري لـ Ubisoft Connect", [AppLanguage.Arabic] = "الخطوة 2: إضافة قواعد حظر الجدار الناري لـ Ubisoft Connect" },
            ["UbiStep2Allow"]        = new() { [AppLanguage.English] = "Step 2: Remove outbound/inbound firewall block rules for Ubisoft Connect", [AppLanguage.French] = "Étape 2: Supprimer les règles de blocage de pare-feu pour Ubisoft Connect", [AppLanguage.Darija] = "الخطوة 2: حيد قواعد الحجب في الجدار الناري لـ Ubisoft Connect", [AppLanguage.Arabic] = "الخطوة 2: إزالة قواعد حظر الجدار الناري لـ Ubisoft Connect" },
            ["UbiStep3"]             = new() { [AppLanguage.English] = "Step 3: Re-enable original network adapters & restore Airplane Mode", [AppLanguage.French] = "Étape 3: Réactiver les cartes réseau d'origine et restaurer le mode avion", [AppLanguage.Darija] = "الخطوة 3: شعل عاوتاني الكارط ريزو ورجع وضع الطيران كيف كان", [AppLanguage.Arabic] = "الخطوة 3: إعادة تمكين محولات الشبكة الأصلية واستعادة وضع الطيران" },
            ["UbiStep4Block"]        = new() { [AppLanguage.English] = "Step 4: Block completed successfully!", [AppLanguage.French] = "Étape 4: Blocage terminé avec succès !", [AppLanguage.Darija] = "الخطوة 4: الحجب كمل بنجاح!", [AppLanguage.Arabic] = "الخطوة 4: تم الحظر بنجاح!" },
            ["UbiStep4Allow"]        = new() { [AppLanguage.English] = "Step 4: Unblock completed successfully!", [AppLanguage.French] = "Étape 4: Déblocage terminé avec succès !", [AppLanguage.Darija] = "الخطوة 4: إلغاء الحجب كمل بنجاح!", [AppLanguage.Arabic] = "الخطوة 4: تم إلغاء الحظر بنجاح!" },

            // Progress status
            ["UbiStatusPrepBlock"]   = new() { [AppLanguage.English] = "Preparing to block...", [AppLanguage.French] = "Préparation du blocage...", [AppLanguage.Darija] = "كنوجدو للحجب...", [AppLanguage.Arabic] = "جاري التحضير للحظر..." },
            ["UbiStatusPrepAllow"]   = new() { [AppLanguage.English] = "Preparing to allow...", [AppLanguage.French] = "Préparation de l'autorisation...", [AppLanguage.Darija] = "كنوجدو للسماح...", [AppLanguage.Arabic] = "جاري التحضير للسماح..." },
            ["UbiStatusStep1"]       = new() { [AppLanguage.English] = "Step 1: Disabling active network adapters & enabling Airplane Mode...", [AppLanguage.French] = "Étape 1: Désactivation des cartes réseau et activation du mode avion...", [AppLanguage.Darija] = "الخطوة 1: كنطفيو الكارط ريزو وكنشغلو وضع الطيران...", [AppLanguage.Arabic] = "الخطوة 1: جاري تعطيل محولات الشبكة وتفعيل وضع الطيران..." },
            ["UbiStatusStep2Block"]  = new() { [AppLanguage.English] = "Step 2: Adding outbound/inbound firewall block rules...", [AppLanguage.French] = "Étape 2: Ajout des règles de blocage de pare-feu...", [AppLanguage.Darija] = "الخطوة 2: كنزيدو قواعد الحجب...", [AppLanguage.Arabic] = "الخطوة 2: جاري إضافة قواعد حظر الجدار الناري..." },
            ["UbiStatusStep2Allow"]  = new() { [AppLanguage.English] = "Step 2: Removing firewall block rules...", [AppLanguage.French] = "Étape 2: Suppression des règles de blocage de pare-feu...", [AppLanguage.Darija] = "الخطوة 2: كنحيدو قواعد الحجب...", [AppLanguage.Arabic] = "الخطوة 2: جاري إزالة قواعد حظر الجدار الناري..." },
            ["UbiStatusStep3"]       = new() { [AppLanguage.English] = "Step 3: Restoring network adapters & Airplane Mode...", [AppLanguage.French] = "Étape 3: Restauration des cartes réseau et du mode avion...", [AppLanguage.Darija] = "الخطوة 3: كنرجعو الكارط ريزو ووضع الطيران...", [AppLanguage.Arabic] = "الخطوة 3: جاري استعادة محولات الشبكة ووضع الطيران..." },
            ["UbiStatusSuccessBlock"] = new() { [AppLanguage.English] = "Success! Ubisoft Connect has been blocked.", [AppLanguage.French] = "Succès ! Ubisoft Connect a été bloqué.", [AppLanguage.Darija] = "ناجح! Ubisoft Connect تم الحجب ديالو.", [AppLanguage.Arabic] = "نجاح! تم حظر Ubisoft Connect." },
            ["UbiStatusSuccessAllow"] = new() { [AppLanguage.English] = "Success! Ubisoft Connect has been allowed.", [AppLanguage.French] = "Succès ! Ubisoft Connect a été autorisé.", [AppLanguage.Darija] = "ناجح! Ubisoft Connect تم السماح ليه.", [AppLanguage.Arabic] = "نجاح! تم السماح لـ Ubisoft Connect." },

            // Labels
            ["UbiFolderLabel"]       = new() { [AppLanguage.English] = "Ubisoft Folder:",             [AppLanguage.French] = "Dossier Ubisoft :",                [AppLanguage.Darija] = "مجلد Ubisoft:", [AppLanguage.Arabic] = "مجلد Ubisoft:" },
            ["EpicFolderLabel"]      = new() { [AppLanguage.English] = "Epic Folder:",                [AppLanguage.French] = "Dossier Epic :",                   [AppLanguage.Darija] = "مجلد Epic:", [AppLanguage.Arabic] = "مجلد Epic:" },
            ["EaFolderLabel"]        = new() { [AppLanguage.English] = "EA Desktop Folder:",          [AppLanguage.French] = "Dossier EA Desktop :",             [AppLanguage.Darija] = "مجلد EA Desktop:", [AppLanguage.Arabic] = "مجلد EA Desktop:" },

            // Growls
            ["GrowlBlockedUbi"]      = new() { [AppLanguage.English] = "Blocked Ubisoft Connect",      [AppLanguage.French] = "Ubisoft Connect bloqué",           [AppLanguage.Darija] = "تم حظر Ubisoft Connect", [AppLanguage.Arabic] = "تم حظر Ubisoft Connect" },
            ["GrowlAllowedUbi"]      = new() { [AppLanguage.English] = "Allowed Ubisoft Connect",      [AppLanguage.French] = "Ubisoft Connect autorisé",         [AppLanguage.Darija] = "تم السماح لـ Ubisoft Connect", [AppLanguage.Arabic] = "تم السماح لـ Ubisoft Connect" },
            ["GrowlBlockedEpic"]     = new() { [AppLanguage.English] = "Blocked Epic Games",           [AppLanguage.French] = "Epic Games bloqué",                [AppLanguage.Darija] = "تم حظر Epic Games", [AppLanguage.Arabic] = "تم حظر Epic Games" },
            ["GrowlAllowedEpic"]     = new() { [AppLanguage.English] = "Allowed Epic Games",           [AppLanguage.French] = "Epic Games autorisé",              [AppLanguage.Darija] = "تم السماح لـ Epic Games", [AppLanguage.Arabic] = "تم السماح لـ Epic Games" },
            ["GrowlBlockedEa"]       = new() { [AppLanguage.English] = "Blocked EA Desktop",           [AppLanguage.French] = "EA Desktop bloqué",                [AppLanguage.Darija] = "تم حظر EA Desktop", [AppLanguage.Arabic] = "تم حظر EA Desktop" },
            ["GrowlAllowedEa"]       = new() { [AppLanguage.English] = "Allowed EA Desktop",           [AppLanguage.French] = "EA Desktop autorisé",              [AppLanguage.Darija] = "تم السماح لـ EA Desktop", [AppLanguage.Arabic] = "تم السماح لـ EA Desktop" },
        };

        public static string Get(string key)
        {
            if (_strings.TryGetValue(key, out var langs) && langs.TryGetValue(Current, out var val))
                return val;
            if (_strings.TryGetValue(key, out var fallback) && fallback.TryGetValue(AppLanguage.English, out var eng))
                return eng;
            return key;
        }

        /// <summary>Formats a localized string with string.Format arguments.</summary>
        public static string GetF(string key, params object[] args)
        {
            var template = Get(key);
            try { return string.Format(template, args); }
            catch { return template; }
        }

        public static bool IsRtl => Current == AppLanguage.Darija || Current == AppLanguage.Arabic;
    }
}
