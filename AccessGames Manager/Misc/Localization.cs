using System.Collections.Generic;

namespace AccessGamesManager.Misc
{
    public enum AppLanguage { English, French, Darija }

    public static class Localization
    {
        public static AppLanguage Current { get; private set; } = AppLanguage.English;

        public static void SetLanguage(AppLanguage lang) => Current = lang;

        private static readonly Dictionary<string, Dictionary<AppLanguage, string>> _strings = new()
        {
            // ── Top bar ───────────────────────────────────────────────────────
            ["AddNewAccount"]        = new() { [AppLanguage.English] = "＋  Add New Account",        [AppLanguage.French] = "＋  Ajouter un compte",           [AppLanguage.Darija] = "＋  زيد حساب جديد"               },

            // ── Nav ───────────────────────────────────────────────────────────
            ["NavGames"]             = new() { [AppLanguage.English] = "🎮  Games",                   [AppLanguage.French] = "🎮  Jeux",                         [AppLanguage.Darija] = "🎮  الألعاب"                     },
            ["NavStore"]             = new() { [AppLanguage.English] = "🛒  Store",                   [AppLanguage.French] = "🛒  Boutique",                     [AppLanguage.Darija] = "🛒  المتجر"                      },
            ["NavAccounts"]          = new() { [AppLanguage.English] = "👤  Accounts",                [AppLanguage.French] = "👤  Comptes",                      [AppLanguage.Darija] = "👤  الحسابات"                    },
            ["NavSettings"]          = new() { [AppLanguage.English] = "⚙  Settings",                [AppLanguage.French] = "⚙  Paramètres",                   [AppLanguage.Darija] = "⚙  الإعدادات"                   },

            // ── Games page ────────────────────────────────────────────────────
            ["Library"]              = new() { [AppLanguage.English] = "Library",                     [AppLanguage.French] = "Bibliothèque",                     [AppLanguage.Darija] = "المكتبة"                         },
            ["GamesCount"]           = new() { [AppLanguage.English] = "games",                       [AppLanguage.French] = "jeux",                             [AppLanguage.Darija] = "لعبة"                            },
            ["RefreshGames"]         = new() { [AppLanguage.English] = "↺  Refresh",                  [AppLanguage.French] = "↺  Actualiser",                    [AppLanguage.Darija] = "↺  تحديث"                        },
            ["GameTooltipOwner"]     = new() { [AppLanguage.English] = "Owner",                       [AppLanguage.French] = "Propriétaire",                     [AppLanguage.Darija] = "صاحب الحساب"                     },
            ["GameTooltipLaunch"]    = new() { [AppLanguage.English] = "Click to launch",             [AppLanguage.French] = "Cliquer pour lancer",              [AppLanguage.Darija] = "كليك باش تلعب"                   },
            ["UnknownOwner"]         = new() { [AppLanguage.English] = "Unknown",                     [AppLanguage.French] = "Inconnu",                          [AppLanguage.Darija] = "مجهول"                           },
            ["PlayBtn"]              = new() { [AppLanguage.English] = "▶  Play",                      [AppLanguage.French] = "▶  Jouer",                         [AppLanguage.Darija] = "▶  العب"                         },
            ["ResetPlaytime"]        = new() { [AppLanguage.English] = "⏱  Reset Playtime",            [AppLanguage.French] = "⏱  Réinit. le temps de jeu",      [AppLanguage.Darija] = "⏱  مسح وقت اللعب"               },
            ["ResetAchievements"]    = new() { [AppLanguage.English] = "🏆  Reset Achievements",        [AppLanguage.French] = "🏆  Réinit. les succès",           [AppLanguage.Darija] = "🏆  مسح الإنجازات"               },
            ["ChangeAccount"]        = new() { [AppLanguage.English] = "Launch via account:",           [AppLanguage.French] = "Lancer avec le compte :",          [AppLanguage.Darija] = "شغّل بالحساب:"                   },
            ["DefaultAccount"]       = new() { [AppLanguage.English] = "Default: {0}",                 [AppLanguage.French] = "Par défaut : {0}",                  [AppLanguage.Darija] = "الافتراضي: {0}"                  },

            // ── Accounts page ─────────────────────────────────────────────────
            ["Accounts"]             = new() { [AppLanguage.English] = "Accounts",                    [AppLanguage.French] = "Comptes",                          [AppLanguage.Darija] = "الحسابات"                        },
            ["AccountsCount"]        = new() { [AppLanguage.English] = "accounts",                    [AppLanguage.French] = "comptes",                          [AppLanguage.Darija] = "حساب"                            },
            ["RefreshAccounts"]      = new() { [AppLanguage.English] = "↺  Refresh",                  [AppLanguage.French] = "↺  Actualiser",                    [AppLanguage.Darija] = "↺  تحديث"                        },

            // Account card role badges
            ["RolePersonal"]         = new() { [AppLanguage.English] = "⭐ Personal",                 [AppLanguage.French] = "⭐ Personnel",                     [AppLanguage.Darija] = "⭐ شخصي"                         },
            ["RoleAccess"]           = new() { [AppLanguage.English] = "🎮 Access",                   [AppLanguage.French] = "🎮 Accès",                         [AppLanguage.Darija] = "🎮 أكسس"                         },

            // Account card switch button
            ["SwitchBtnOnline"]      = new() { [AppLanguage.English] = "Login Normally",              [AppLanguage.French] = "Connexion normale",                [AppLanguage.Darija] = "دخل بشكل عادي"                   },
            ["SwitchBtnOffline"]     = new() { [AppLanguage.English] = "Switch (Offline)",            [AppLanguage.French] = "Changer (Hors ligne)",             [AppLanguage.Darija] = "بدل (أوفلاين)"                   },
            ["SwitchTooltip"]        = new() { [AppLanguage.English] = "Switch to",                   [AppLanguage.French] = "Changer vers",                     [AppLanguage.Darija] = "بدل إلى"                         },

            // ── Settings page ─────────────────────────────────────────────────
            ["SettingsTitle"]        = new() { [AppLanguage.English] = "Settings",                    [AppLanguage.French] = "Paramètres",                       [AppLanguage.Darija] = "الإعدادات"                       },
            ["LanguageSection"]      = new() { [AppLanguage.English] = "LANGUAGE",                    [AppLanguage.French] = "LANGUE",                           [AppLanguage.Darija] = "اللغة"                           },
            ["LanguageLabel"]        = new() { [AppLanguage.English] = "Language",                    [AppLanguage.French] = "Langue",                           [AppLanguage.Darija] = "اللغة"                           },
            ["NetworkSection"]       = new() { [AppLanguage.English] = "NETWORK",                     [AppLanguage.French] = "RÉSEAU",                           [AppLanguage.Darija] = "الشبكة"                          },
            ["FirewallControl"]      = new() { [AppLanguage.English] = "Firewall Control",            [AppLanguage.French] = "Contrôle du pare-feu",             [AppLanguage.Darija] = "التحكم في الجدار الناري"         },
            ["FirewallDescOn"]       = new() { [AppLanguage.English] = "Steam can currently access the internet.",         [AppLanguage.French] = "Steam peut actuellement accéder à Internet.",         [AppLanguage.Darija] = "ستيم قادر يوصل للإنترنت دابا."               },
            ["FirewallDescOff"]      = new() { [AppLanguage.English] = "Steam is blocked — it cannot reach the internet.", [AppLanguage.French] = "Steam est bloqué — il ne peut pas accéder à Internet.", [AppLanguage.Darija] = "ستيم محجوب — ما قادرش يوصل للإنترنت."       },
            ["BlockSteam"]           = new() { [AppLanguage.English] = "🔒  Block Steam",             [AppLanguage.French] = "🔒  Bloquer Steam",                [AppLanguage.Darija] = "🔒  حجب ستيم"                    },
            ["AllowSteam"]           = new() { [AppLanguage.English] = "🔓  Allow Steam",             [AppLanguage.French] = "🔓  Autoriser Steam",              [AppLanguage.Darija] = "🔓  سمح لستيم"                   },
            ["GrowlBlocked"]         = new() { [AppLanguage.English] = "Steam blocked — firewall rule applied.",           [AppLanguage.French] = "Steam bloqué — règle de pare-feu appliquée.",        [AppLanguage.Darija] = "ستيم تحجب — القاعدة ديال الجدار الناري تطبقات." },
            ["GrowlUnblocked"]       = new() { [AppLanguage.English] = "Steam unblocked — firewall rule removed.",        [AppLanguage.French] = "Steam débloqué — règle de pare-feu supprimée.",      [AppLanguage.Darija] = "ستيم تحل — القاعدة ديال الجدار الناري تمسحات." },
            ["LaunchModeSection"]    = new() { [AppLanguage.English] = "LAUNCH MODE",                 [AppLanguage.French] = "MODE DE LANCEMENT",               [AppLanguage.Darija] = "وضع التشغيل"                     },
            ["ForceLaunchMode"]      = new() { [AppLanguage.English] = "Force Launch Mode",           [AppLanguage.French] = "Mode de lancement forcé",          [AppLanguage.Darija] = "إجبار وضع التشغيل"               },
            ["ForceLaunchDesc"]      = new() { [AppLanguage.English] = "Override the per-account role logic. Auto uses the account\u2019s role (Personal = Online, Access = Offline).", [AppLanguage.French] = "Remplacer la logique de r\u00f4le par compte. Auto utilise le r\u00f4le du compte (Personnel = En ligne, Acc\u00e8s = Hors ligne).", [AppLanguage.Darija] = "بدل منطق الدور ديال الحساب. أوتو كيستعمل دور الحساب (شخصي = أونلاين، أكسس = أوفلاين)." },
            ["LaunchAuto"]           = new() { [AppLanguage.English] = "⚡  Auto  (use account role)", [AppLanguage.French] = "⚡  Auto  (utiliser le rôle du compte)", [AppLanguage.Darija] = "⚡  تلقائي  (حسب دور الحساب)"  },
            ["LaunchForceOnline"]    = new() { [AppLanguage.English] = "🌐  Force Online  (always online)",   [AppLanguage.French] = "🌐  Forcer En ligne  (toujours en ligne)",    [AppLanguage.Darija] = "🌐  أونلاين دايما"               },
            ["LaunchForceOffline"]   = new() { [AppLanguage.English] = "🔒  Force Offline  (always offline)", [AppLanguage.French] = "🔒  Forcer Hors ligne  (toujours hors ligne)", [AppLanguage.Darija] = "🔒  أوفلاين دايما"               },
            ["AboutSection"]         = new() { [AppLanguage.English] = "ABOUT",                       [AppLanguage.French] = "À PROPOS",                         [AppLanguage.Darija] = "علينا"                           },
            ["AboutDesc"]            = new() { [AppLanguage.English] = "Manage your game library and accounts. Switch accounts with automatic firewall control to keep your session active.", [AppLanguage.French] = "Gérez votre bibliothèque de jeux et vos comptes. Changez de compte avec contrôle automatique du pare-feu.", [AppLanguage.Darija] = "دير في مكتبة الألعاب والحسابات ديالك. بدل الحسابات مع تحكم تلقائي في الجدار الناري." },

            // ── Status bar ────────────────────────────────────────────────────
            ["StatusReady"]          = new() { [AppLanguage.English] = "Ready",                       [AppLanguage.French] = "Prêt",                             [AppLanguage.Darija] = "جاهز"                            },
            ["StatusLoadingGames"]   = new() { [AppLanguage.English] = "Loading games…",              [AppLanguage.French] = "Chargement des jeux…",             [AppLanguage.Darija] = "كيتحملو الألعاب…"                },
            ["StatusLoadingAccs"]    = new() { [AppLanguage.English] = "Loading accounts…",           [AppLanguage.French] = "Chargement des comptes…",          [AppLanguage.Darija] = "كيتحملو الحسابات…"               },
            ["StatusLoadedGames"]    = new() { [AppLanguage.English] = "Loaded {0} games",            [AppLanguage.French] = "{0} jeux chargés",                 [AppLanguage.Darija] = "تحملو {0} لعبة"                  },
            ["StatusLoadedAccs"]     = new() { [AppLanguage.English] = "Loaded {0} accounts",         [AppLanguage.French] = "{0} comptes chargés",              [AppLanguage.Darija] = "تحملو {0} حساب"                  },
            ["StatusBlocked"]        = new() { [AppLanguage.English] = "🔒 Steam network blocked",    [AppLanguage.French] = "🔒 Réseau Steam bloqué",           [AppLanguage.Darija] = "🔒 شبكة ستيم محجوبة"             },
            ["StatusOpen"]           = new() { [AppLanguage.English] = "🔓 Steam network open",       [AppLanguage.French] = "🔓 Réseau Steam ouvert",           [AppLanguage.Darija] = "🔓 شبكة ستيم مفتوحة"             },
            ["StatusSwitching"]      = new() { [AppLanguage.English] = "Switching to {0}…",           [AppLanguage.French] = "Changement vers {0}…",             [AppLanguage.Darija] = "كيتبدل إلى {0}…"                 },
            ["StatusLaunching"]      = new() { [AppLanguage.English] = "🚀 Launching {0} as {1} ({2})…", [AppLanguage.French] = "🚀 Lancement de {0} en tant que {1} ({2})…", [AppLanguage.Darija] = "🚀 كيتلعب {0} بحساب {1} ({2})…" },
            ["StatusSteamRestart"]   = new() { [AppLanguage.English] = "Steam restarting — login page will appear", [AppLanguage.French] = "Steam redémarre — la page de connexion va apparaître", [AppLanguage.Darija] = "ستيم كيعاود — صفحة الدخول غادي تظهر" },
            ["StatusLaunchModeAuto"] = new() { [AppLanguage.English] = "Launch mode: Auto (uses account role)",    [AppLanguage.French] = "Mode de lancement : Auto (rôle du compte)",          [AppLanguage.Darija] = "وضع التشغيل: تلقائي (حسب دور الحساب)"    },
            ["StatusLaunchOnline"]   = new() { [AppLanguage.English] = "Launch mode: Force Online",               [AppLanguage.French] = "Mode de lancement : Forcer En ligne",                [AppLanguage.Darija] = "وضع التشغيل: أونلاين دايما"               },
            ["StatusLaunchOffline"]  = new() { [AppLanguage.English] = "Launch mode: Force Offline",              [AppLanguage.French] = "Mode de lancement : Forcer Hors ligne",              [AppLanguage.Darija] = "وضع التشغيل: أوفلاين دايما"               },
            ["NoAccountToLaunch"]    = new() { [AppLanguage.English] = "No account found to launch {0}.",         [AppLanguage.French] = "Aucun compte trouvé pour lancer {0}.",               [AppLanguage.Darija] = "ما لقيناش حساب باش نلعبو {0}."           },

            // ── Already signed in dialog ────────────────────────────────────────────────────────
            ["AlreadySignedInTitle"] = new() { [AppLanguage.English] = "Account already signed in",         [AppLanguage.French] = "Compte déjà connecté",              [AppLanguage.Darija] = "الحساب دخل بالفعل"          },
            ["AlreadySignedInMsg"]   = new() { [AppLanguage.English] = "{0} is already the active Steam account. What do you want to do?", [AppLanguage.French] = "{0} est déjà le compte Steam actif. Que voulez-vous faire ?", [AppLanguage.Darija] = "{0} دخلاتي بالفعل. شنو بغيتي ديري ؟" },
            ["AlreadySignedInReboot"]= new() { [AppLanguage.English] = "Reboot Steam",                      [AppLanguage.French] = "Redémarrer Steam",                [AppLanguage.Darija] = "عاود تشغيل ستيم"              },
            ["AlreadySignedInLaunch"]= new() { [AppLanguage.English] = "Just launch the game",              [AppLanguage.French] = "Lancer le jeu seulement",         [AppLanguage.Darija] = "لانساب غير تشغيل ستيم"      },
            ["AlreadySignedInCancel"]= new() { [AppLanguage.English] = "Cancel",                             [AppLanguage.French] = "Annuler",                          [AppLanguage.Darija] = "إلغاء"                          },

            // ── Network pill ──────────────────────────────────────────────────
            ["Online"]               = new() { [AppLanguage.English] = "Online",                      [AppLanguage.French] = "En ligne",                         [AppLanguage.Darija] = "أونلاين"                         },
            ["Offline"]              = new() { [AppLanguage.English] = "Offline",                     [AppLanguage.French] = "Hors ligne",                       [AppLanguage.Darija] = "أوفلاين"                         },
            ["ONLINE"]               = new() { [AppLanguage.English] = "ONLINE",                      [AppLanguage.French] = "EN LIGNE",                         [AppLanguage.Darija] = "أونلاين"                         },
            ["OFFLINE"]              = new() { [AppLanguage.English] = "OFFLINE",                     [AppLanguage.French] = "HORS LIGNE",                       [AppLanguage.Darija] = "أوفلاين"                         },

            // ── App version footer ────────────────────────────────────────────
            ["AppVersion"]           = new() { [AppLanguage.English] = "AccessGames Manager v2.5.1",    [AppLanguage.French] = "AccessGames Manager v2.5.1",         [AppLanguage.Darija] = "AccessGames Manager v2.5.1"        },
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

        public static bool IsRtl => Current == AppLanguage.Darija;
    }
}
