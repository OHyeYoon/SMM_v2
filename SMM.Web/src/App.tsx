import {
    X,
    Check,
    HeartPulse,
    NotepadText,
    Play,
    User,
    Ellipsis,
    Settings,
    LoaderCircle,
    FolderOpen,
    Coffee,
    SquareArrowOutUpLeft,
} from "lucide-react";
import "./App.css";
import { Button } from "./components/ui/button";
import { Label } from "./components/ui/label";
import { Separator } from "./components/ui/separator";
import { useEffect, useState } from "react";
import { Badge } from "./components/ui/badge";
import {
    Tooltip,
    TooltipTrigger,
    TooltipContent,
} from "./components/ui/tooltip";
import {
    DropdownMenu,
    DropdownMenuContent,
    DropdownMenuItem,
    DropdownMenuTrigger,
} from "./components/ui/dropdown-menu";
import {
    Dialog,
    DialogContent,
    DialogFooter,
    DialogHeader,
    DialogTitle,
    DialogTrigger,
} from "./components/ui/dialog";
import { Input } from "./components/ui/input";
import {
    Select,
    SelectTrigger,
    SelectValue,
    SelectContent,
    SelectItem,
} from "./components/ui/select";

type GameInfo = {
    isInstalled: boolean;
    gamePath: string;
    executablePath: string;
    version: string;
    errorMessage: string;
};

type SmapiInfo = {
    isInstalled: boolean;
    smapiPath: string;
    executablePath: string;
    version: string;
    errorMessage: string;
};

type ModInfo = {
    name: string;
    author: string;
    version: string;
    uniqueId: string;
    nexusUrl: string;
    description: string;
    folderName: string;
    folderPath: string;
    manifestPath: string;
    hasManifest: boolean;
    isValidManifest: boolean;
    isEnabled: boolean;
    status: string;
    errorMessage: string;

    issues?: { type: string; target: string; message: string }[];
};

type ProfileInfo = {
    name: string;
    isActive: boolean;
    folderPath: string;
    exists: boolean;
};

function App() {
    const [isLocked, setIsLocked] = useState(false);
    const [lockMessage, setLockMessage] = useState(``);

    const [lang, setLang] = useState<Record<string, string>>({});
    const t = (key: string, values?: Record<string, string | number>) => {
        let text = lang[key] ?? key;

        if (values) {
            Object.entries(values).forEach(([name, value]) => {
                text = text.replaceAll(`{${name}}`, String(value));
            });
        }

        return text;
    };

    const [keyword, setKeyword] = useState(``);
    const [filter, setFilter] = useState(`all`);
    const [sort, setSort] = useState("name");
    const [order, setOrder] = useState("Descending");

    const [isSettingsDialogOpen, setIsSettingsDialogOpen] = useState(false);
    const [settings, setSettings] = useState({
        language: "en",
        theme: "light",
    });

    useEffect(() => {
        const root = document.documentElement;

        if (settings.theme === "dark") {
            root.classList.add("dark");
        } else {
            root.classList.remove("dark");
        }
    }, [settings.theme]);

    const [gameInfo, setGameInfo] = useState<GameInfo | null>(null);
    const [smapiInfo, setSmapiInfo] = useState<SmapiInfo | null>(null);
    const [mods, setMods] = useState<ModInfo[]>([]);
    const totalCount = mods.length;
    const enabledCount = mods.filter((m) => m.isEnabled).length;
    const disabledCount = totalCount - enabledCount;
    const warningCount = mods.filter((m) => m.status === "Warning").length;
    const errorCount = mods.filter((m) => m.status === "Error").length;

    const [profiles, setProfiles] = useState<ProfileInfo[]>([]);
    const [isProfileDialogOpen, setIsProfileDialogOpen] = useState(false);
    const activeProfile = profiles.find((p) => p.isActive);

    const [newProfileName, setNewProfileName] = useState(``);
    const [isNewProfileDialogOpen, setIsNewProfileDialogOpen] = useState(false);

    const [editProfileName, setEditProfileName] = useState(``);
    const [isEditProfileDialogOpen, setIsEditProfileDialogOpen] =
        useState(false);

    const [selectedProfileName, setSelectedProfileName] = useState<
        string | null
    >(null);
    const selectedProfile =
        profiles.find((p) => p.name === selectedProfileName) ?? activeProfile;

    const [version, setVersion] = useState("");

    useEffect(() => {
        window.external.receiveMessage((message: string) => {
            const response = JSON.parse(message);

            switch (response.type) {
                case "game":
                    setGameInfo(response.data);
                    break;

                case "smapi":
                    setSmapiInfo(response.data);
                    break;

                case "mods":
                    setMods(response.data);
                    break;

                case "profiles":
                    setProfiles(response.data);
                    break;

                case "profileAction":
                    setIsLocked(false);
                    break;

                case "gameRunning":
                    setIsLocked(response.data.running);
                    setLockMessage(
                        response.data.running ? t("Lock.GameRunning") : ``,
                    );
                    break;

                case "appVersion":
                    setVersion(response.data);
                    break;

                case "language":
                    setLang(response.data);
                    break;

                case "settings":
                    setSettings(response.data);
                    break;
            }
        });

        window.external.sendMessage("game.getInfo");
        window.external.sendMessage("smapi.getInfo");
        window.external.sendMessage("mods.getList");
        window.external.sendMessage("app.getVersion");
        window.external.sendMessage("profiles.getList");
        window.external.sendMessage("language.get");
        window.external.sendMessage("settings.get");
    }, []);

    const filteredMods = mods
        .filter((mod) => {
            const searchText = `
            ${mod.name}
            ${mod.author}
            ${mod.uniqueId}
            ${mod.folderName}
            ${mod.version}
        `.toLowerCase();

            if (!searchText.includes(keyword.trim().toLowerCase())) {
                return false;
            }

            switch (filter) {
                case "enabled":
                    return mod.isEnabled;

                case "disabled":
                    return !mod.isEnabled;

                case "warning":
                    return mod.status === "Warning";

                case "error":
                    return mod.status === "Error";

                default:
                    return true;
            }
        })
        .sort((a, b) => {
            switch (sort) {
                case "enabled":
                    return Number(a.isEnabled) - Number(b.isEnabled);

                case "status": {
                    const statusOrder: Record<string, number> = {
                        Error: 0,
                        Warning: 1,
                        Normal: 2,
                        Disabled: 3,
                    };

                    return (
                        (statusOrder[a.status] ?? 99) -
                        (statusOrder[b.status] ?? 99)
                    );
                }

                default:
                    return a.name.localeCompare(b.name);
            }
        });

    if (order === "Descending") {
        filteredMods.reverse();
    }

    return (
        <>
            {isLocked && (
                <div
                    className={`
                        absolute inset-0
                        z-150
                        flex
                        flex-col
                        items-center
                        justify-center
                        bg-card/70
                        backdrop-blur-sm
                    `}
                >
                    <LoaderCircle
                        className={`size-24 animate-spin mb-3 text-primary`}
                    />

                    <div className={`text-sm font-medium`}>
                        {lockMessage ? t(lockMessage) : ``}
                    </div>
                </div>
            )}
            <div
                className={`flex flex-row gap-4 p-4 w-full h-svh min-h-0 overflow-auto`}
            >
                <div className={`flex min-w-0 min-h-0 h-full flex-1 flex-col`}>
                    <div
                        className={`mb-2 flex h-10 shrink-0 items-center gap-2`}
                    >
                        <Input
                            value={keyword}
                            onChange={(e) => setKeyword(e.target.value)}
                            placeholder={t("Mod.Search")}
                            className={`flex-1 rounded-[4px] focus:outline-none
                                focus:ring-0
                                focus-visible:outline-none
                                focus-visible:ring-0
                                focus-visible:ring-offset-0`}
                        />

                        <Select value={filter} onValueChange={setFilter}>
                            <SelectTrigger
                                className={`w-30 rounded-[4px] text-[14px] focus:outline-none
                                focus:ring-0
                                focus-visible:outline-none
                                focus-visible:ring-0
                                focus-visible:ring-offset-0 `}
                            >
                                <SelectValue />
                            </SelectTrigger>
                            <SelectContent>
                                <SelectItem
                                    className={`focus:bg-card hover:bg-transparent text-[14px] h-10
                                                        px-3
                                                        text-[14px]
                                                        focus:bg-muted
                                                        data-[highlighted]:bg-muted`}
                                    value={`all`}
                                >
                                    {t("Mod.Filter.All")}
                                </SelectItem>
                                <SelectItem value={`enabled`} className={`focus:bg-card hover:bg-transparent text-[14px] h-10
                                                        px-3
                                                        text-[14px]
                                                        focus:bg-muted
                                                        data-[highlighted]:bg-muted`}> 
                                    {t("Mod.Filter.Enabled")}
                                </SelectItem>
                                <SelectItem value={`disabled`} className={`focus:bg-card hover:bg-transparent text-[14px] h-10
                                                        px-3
                                                        text-[14px]
                                                        focus:bg-muted
                                                        data-[highlighted]:bg-muted`}>
                                    {t("Mod.Filter.Disabled")}
                                </SelectItem>
                                <SelectItem value={`warning`} className={`focus:bg-card hover:bg-transparent text-[14px] h-10
                                                        px-3
                                                        text-[14px]
                                                        focus:bg-muted
                                                        data-[highlighted]:bg-muted`}>
                                    {t("Mod.Filter.Warning")}
                                </SelectItem>
                                <SelectItem value={`error`} className={`focus:bg-card hover:bg-transparent text-[14px] h-10
                                                        px-3
                                                        text-[14px]
                                                        focus:bg-muted
                                                        data-[highlighted]:bg-muted`}>
                                    {t("Mod.Filter.Error")}
                                </SelectItem>
                            </SelectContent>
                        </Select>

                        <Select value={sort} onValueChange={setSort}>
                            <SelectTrigger
                                className={`w-30 rounded-[4px] text-[14px] focus:outline-none
                                focus:ring-0
                                focus-visible:outline-none
                                focus-visible:ring-0
                                focus-visible:ring-offset-0`}
                            >
                                <SelectValue />
                            </SelectTrigger>
                            <SelectContent>
                                <SelectItem value={`name`} className={`text-[14px] h-10
                                                        px-3
                                                        text-[14px]
                                                        focus:bg-muted
                                                        data-[highlighted]:bg-muted`}>
                                    {t("Mod.Sort.Name")}
                                </SelectItem>
                                <SelectItem value={`status`} className={`text-[14px] h-10
                                                        px-3
                                                        text-[14px]
                                                        focus:bg-muted
                                                        data-[highlighted]:bg-muted`}>
                                    {t("Mod.Sort.Status")}
                                </SelectItem>
                            </SelectContent>
                        </Select>

                        <Select value={order} onValueChange={setOrder}>
                            <SelectTrigger
                                className={`w-30 rounded-[4px] text-[14px] focus:outline-none
                                focus:ring-0
                                focus-visible:outline-none
                                focus-visible:ring-0
                                focus-visible:ring-offset-0`}
                            >
                                <SelectValue />
                            </SelectTrigger>
                            <SelectContent>
                                <SelectItem value={`Ascending`} className={`text-[14px] h-10
                                                        px-3
                                                        text-[14px]
                                                        focus:bg-muted
                                                        data-[highlighted]:bg-muted`}>
                                    {t("Mod.Order.Asc")}
                                </SelectItem>
                                <SelectItem value={`Descending`} className={`text-[14px] h-10
                                                        px-3
                                                        text-[14px]
                                                        focus:bg-muted
                                                        data-[highlighted]:bg-muted`}>
                                    {t("Mod.Order.Desc")}
                                </SelectItem>
                            </SelectContent>
                        </Select>

                        <Button
                            variant={`ghost`}
                            className={`rounded-[4px]`}
                            onClick={() => {
                                window.external.sendMessage(`mods.openFolder`);
                            }}
                        >
                            <FolderOpen />
                        </Button>

                        <Dialog
                            open={isSettingsDialogOpen}
                            onOpenChange={setIsSettingsDialogOpen}
                        >
                            <DialogTrigger asChild>
                                <Button
                                    variant={`ghost`}
                                    className={`rounded-[4px]`}
                                    onClick={() => {}}
                                >
                                    <Settings />
                                </Button>
                            </DialogTrigger>

                            <DialogContent
                                className={`sm:max-w-[400px] h-[320px] flex flex-col gap-0`}
                                onInteractOutside={(e) => {
                                    e.preventDefault();
                                }}
                            >
                                <DialogHeader className={`flex-none mb-1.5`}>
                                    <DialogTitle
                                        className={`!text-[20px] !text-foreground flex-none mb-auto font-sans`}
                                    >
                                        {t("App.Settings")}
                                    </DialogTitle>
                                </DialogHeader>

                                <div className={`flex flex-col min-h-0 flex-1`}>
                                    <div
                                        className={`p-4 grid grid-cols-[100px_auto] gap-2`}
                                    >
                                        <Label className={`text-[15px]`}>
                                            {t("Settings.Language")}
                                        </Label>
                                        <Select
                                            value={settings.language}
                                            onValueChange={(value) =>
                                                setSettings({
                                                    ...settings,
                                                    language: value,
                                                })
                                            }
                                        >
                                            <SelectTrigger
                                                className={`w-full rounded-[4px] text-[14px] focus:outline-none
                                                        focus:ring-0
                                                        focus-visible:outline-none
                                                        focus-visible:ring-0
                                                        focus-visible:ring-offset-0`}
                                            >
                                                <SelectValue />
                                            </SelectTrigger>
                                            <SelectContent>
                                                <SelectItem
                                                    value={`en`}
                                                    className={`text-[14px]  h-10
                                                        px-3
                                                        text-[14px]
                                                        focus:bg-muted
                                                        data-[highlighted]:bg-muted`}
                                                >
                                                    English
                                                </SelectItem>
                                                <SelectItem
                                                    value={`ko`}
                                                    className={`text-[14px]  h-10
                                                        px-3
                                                        text-[14px]
                                                        focus:bg-muted
                                                        data-[highlighted]:bg-muted`}
                                                >
                                                    한국어
                                                </SelectItem>
                                            </SelectContent>
                                        </Select>
                                    </div>

                                    <div
                                        className={`p-4 grid grid-cols-[100px_auto] gap-2`}
                                    >
                                        <Label className={`text-[15px]`}>
                                            {t("Settings.Theme")}
                                        </Label>
                                        <Select
                                            value={settings.theme}
                                            onValueChange={(value) =>
                                                setSettings({
                                                    ...settings,
                                                    theme: value,
                                                })
                                            }
                                        >
                                            <SelectTrigger
                                                className={`w-full rounded-[4px] text-[14px] focus:outline-none
                                                        focus:ring-0
                                                        focus-visible:outline-none
                                                        focus-visible:ring-0
                                                        focus-visible:ring-offset-0`}
                                            >
                                                <SelectValue />
                                            </SelectTrigger>
                                            <SelectContent
                                                className={`
                                                    w-[var(--radix-select-trigger-width)]
                                                    min-w-[var(--radix-select-trigger-width)]
                                                    rounded-[4px]
                                                    p-0
                                                    overflow-hidden
                                                `}
                                            >
                                                <SelectItem
                                                    value={`light`}
                                                    className={`text-[14px]  h-10
                                                        px-3
                                                        text-[14px]
                                                        focus:bg-muted
                                                        data-[highlighted]:bg-muted`}
                                                >
                                                    Light
                                                </SelectItem>
                                                <SelectItem
                                                    value={`dark`}
                                                    className={`text-[14px]  h-10
                                                        px-3
                                                        text-[14px]
                                                        focus:bg-muted
                                                        data-[highlighted]:bg-muted`}
                                                >
                                                    Dark
                                                </SelectItem>
                                            </SelectContent>
                                        </Select>
                                    </div>
                                </div>

                                <DialogFooter
                                    className={`bg-card flex justify-end mt-auto flex-none border-none`}
                                >
                                    <Button
                                        onClick={() => {
                                            window.external.sendMessage(
                                                `settings.save|${settings.language}|${settings.theme}`,
                                            );

                                            setIsSettingsDialogOpen(false);
                                        }}
                                    >
                                        {t("Common.Apply")}
                                    </Button>
                                </DialogFooter>
                            </DialogContent>
                        </Dialog>
                    </div>
                    <div
                        className={`flex min-h-0 flex-1 flex-col overflow-hidden rounded-[4px] bg-muted`}
                    >
                        <div
                            className={`flex shrink-0 items-center border-b px-4 py-2 border-border text-sm font-medium text-muted-foreground`}
                        >
                            <div className={`text-center text-[14px]`} />
                            <div className={`flex-1 text-left text-[14px]`}>
                                {t("Mod.Title")}
                            </div>
                            <div
                                className={`text-[12px] text-muted-foreground`}
                            >
                                {t("Mod.CountSummary", {
                                    total: mods.length,
                                    enabled: enabledCount,
                                    disabled: disabledCount,
                                })}
                            </div>
                        </div>

                        <div
                            className={`min-h-0 flex-1 overflow-y-auto overflow-x-hidden px-4 py-2 space-y-2`}
                        >
                            {filteredMods.map((mod) => (
                                <div
                                    key={mod.uniqueId || mod.folderPath}
                                    className={`
                                        group grid min-h-14 w-full grid-cols-[24px_minmax(0,1fr)_110px_32px]
                                        items-center gap-3 rounded-[4px]
                                    `}
                                >
                                    <input
                                        type={`checkbox`}
                                        checked={mod.isEnabled}
                                        onChange={() => {
                                            window.external.sendMessage(
                                                `mods.toggle|${mod.folderPath}`,
                                            );
                                        }}
                                        className={`size-4 accent-blue-600`}
                                    />

                                    <div
                                        className={`w-200 truncate text-[14px] font-medium text-left flex flex-col`}
                                    >
                                        <Label>{mod.name}</Label>
                                        <Label
                                            className={`text-[12px] text-muted-foreground font-normal`}
                                        >
                                            {mod.version || `-`}
                                        </Label>
                                    </div>

                                    <div className={`w-28`}>
                                        {mod.status === `Normal` && (
                                            <Badge
                                                className={`
                                                    bg-green-100
                                                    text-green-700
                                                `}
                                            >
                                                {t("Mod.Status.Normal")}
                                            </Badge>
                                        )}

                                        {mod.status === `Warning` && (
                                            <Tooltip>
                                                <TooltipTrigger asChild>
                                                    <Badge
                                                        className={`
                                                            bg-yellow-100
                                                            text-yellow-700
                                                        `}
                                                    >
                                                        {t(
                                                            "Mod.Status.WarningWithCount",
                                                            {
                                                                count:
                                                                    mod.issues
                                                                        ?.length ??
                                                                    0,
                                                            },
                                                        )}
                                                    </Badge>
                                                </TooltipTrigger>

                                                <TooltipContent
                                                    side={`left`}
                                                    className={`max-w-80`}
                                                >
                                                    <div
                                                        className={`space-y-2`}
                                                    >
                                                        {mod.issues?.map(
                                                            (issue, index) => (
                                                                <div
                                                                    key={index}
                                                                    className={`space-y-1`}
                                                                >
                                                                    <div
                                                                        className={`font-medium`}
                                                                    >
                                                                        {
                                                                            issue.type
                                                                        }
                                                                    </div>

                                                                    <div
                                                                        className={`text-xs text-muted-foreground`}
                                                                    >
                                                                        {
                                                                            issue.message
                                                                        }
                                                                    </div>
                                                                </div>
                                                            ),
                                                        )}
                                                    </div>
                                                </TooltipContent>
                                            </Tooltip>
                                        )}

                                        {mod.status === `Error` && (
                                            <Tooltip>
                                                <TooltipTrigger asChild>
                                                    <Badge
                                                        variant={`destructive`}
                                                    >
                                                        {t(
                                                            "Mod.Status.ErrorWithCount",
                                                            {
                                                                count:
                                                                    mod.issues
                                                                        ?.length ??
                                                                    0,
                                                            },
                                                        )}
                                                    </Badge>
                                                </TooltipTrigger>

                                                <TooltipContent
                                                    side={`left`}
                                                    className={`max-w-80`}
                                                >
                                                    <div
                                                        className={`space-y-2`}
                                                    >
                                                        {mod.issues?.map(
                                                            (issue, index) => (
                                                                <div
                                                                    key={index}
                                                                    className={`space-y-1`}
                                                                >
                                                                    <div
                                                                        className={`font-medium`}
                                                                    >
                                                                        {
                                                                            issue.type
                                                                        }
                                                                    </div>

                                                                    <div
                                                                        className={`text-xs text-muted-foreground`}
                                                                    >
                                                                        {
                                                                            issue.message
                                                                        }
                                                                    </div>
                                                                </div>
                                                            ),
                                                        )}
                                                    </div>
                                                </TooltipContent>
                                            </Tooltip>
                                        )}

                                        {!mod.isEnabled && (
                                            <Badge variant={`secondary`}>
                                                {t("Mod.Status.Disabled")}
                                            </Badge>
                                        )}
                                    </div>

                                    <DropdownMenu>
                                        <DropdownMenuTrigger asChild>
                                            <button
                                                className={`
                                                    ml-auto
                                                    flex size-8 items-center justify-center rounded-[4px]
                                                    opacity-40
                                                    transition-all
                                                    outline-none
                                                    focus:outline-none
                                                    hover:bg-muted
                                                    hover:opacity-100
                                                    data-[state=open]:bg-muted
                                                    data-[state=open]:opacity-100
                                                `}
                                            >
                                                <Ellipsis
                                                    className={`size-4 text-muted-foreground`}
                                                />
                                            </button>
                                        </DropdownMenuTrigger>

                                        <DropdownMenuContent
                                            align={`end`}
                                            className={`
                                                min-w-44
                                                rounded-[6px]
                                                bg-card
                                                p-1
                                                shadow-lg
                                            `}
                                        >
                                            <DropdownMenuItem
                                                className={`
                                                    cursor-pointer
                                                    rounded-[4px]
                                                    px-2
                                                    py-1
                                                    text-[14px]
                                                    focus:bg-muted
                                                `}
                                                onClick={() => {
                                                    window.external.sendMessage(
                                                        `mods.openFolder|${mod.folderPath}`,
                                                    );
                                                }}
                                            >
                                                {t("Mod.OpenFolder")}
                                            </DropdownMenuItem>

                                            <DropdownMenuItem
                                                disabled={!mod.nexusUrl}
                                                className={`
                                                    cursor-pointer
                                                    rounded-[4px]
                                                    px-2
                                                    py-1
                                                    text-[14px]
                                                    focus:bg-muted
                                                    disabled:cursor-not-allowed
                                                    disabled:opacity-40
                                                `}
                                                onClick={() => {
                                                    if (!mod.nexusUrl) return;

                                                    window.external.sendMessage(
                                                        `mods.openNexus|${mod.nexusUrl}`,
                                                    );
                                                }}
                                            >
                                                {t("Mod.OpenNexus")}
                                            </DropdownMenuItem>
                                        </DropdownMenuContent>
                                    </DropdownMenu>
                                </div>
                            ))}
                        </div>
                    </div>
                </div>
                <div className={`w-75 h-full flex flex-col space-y-4`}>
                    <Button
                        className={`w-full h-14 rounded-[4px] text-2xl font-bold flex flex-row items-center justify-center`}
                        onClick={() => {
                            setLockMessage("Lock.StartingGame");
                            setIsLocked(true);

                            window.external.sendMessage("game.start");
                        }}
                    >
                        <Play className={`size-6`} />
                        {t("App.Start")}
                    </Button>

                    <div
                        className={`rounded-[4px] bg-muted flex flex-row space-x-4 p-4 `}
                    >
                        <div
                            className={`w-1/2 flex flex-row items-center justify-start gap-4`}
                        >
                            {!smapiInfo?.isInstalled ? (
                                <div
                                    className={`bg-red-500 w-6 h-6 rounded-full flex items-center justify-center`}
                                >
                                    <X className={`text-white size-4.5`} />
                                </div>
                            ) : (
                                <div
                                    className={`bg-green-500 w-6 h-6 rounded-full flex items-center justify-center`}
                                >
                                    <Check className={`text-white size-4.5`} />
                                </div>
                            )}
                            <div className={`flex flex-col leading-[1.5]`}>
                                <Label
                                    className={`text-[14px] text-left text-nowrap`}
                                >
                                    {t("App.Smapi")}
                                </Label>
                                <Label
                                    className={`text-[12px] text-muted-foreground font-normal`}
                                >
                                    {smapiInfo?.version}
                                </Label>
                            </div>
                        </div>
                        <Separator
                            orientation={`vertical`}
                            className={`hidden md:block bg-border`}
                        />
                        <div
                            className={`w-1/2 flex flex-row items-center justify-start gap-4`}
                        >
                            {!gameInfo?.isInstalled ? (
                                <div
                                    className={`bg-red-500 w-6 h-6 rounded-full flex items-center justify-center`}
                                >
                                    <X className={`text-white size-4.5`} />
                                </div>
                            ) : (
                                <div
                                    className={`bg-green-500 w-6 h-6 rounded-full flex items-center justify-center`}
                                >
                                    <Check className={`text-white size-4.5`} />
                                </div>
                            )}
                            <div className={`flex flex-col leading-[1.5]`}>
                                <Label
                                    className={`text-[14px] text-left text-nowrap`}
                                >
                                    {t("App.Game")}
                                </Label>
                                <Label
                                    className={`text-[12px] text-muted-foreground font-normal`}
                                >
                                    {gameInfo?.version}
                                </Label>
                            </div>
                        </div>
                    </div>

                    <div
                        className={`rounded-[4px] bg-muted flex flex-row p-4 `}
                    >
                        <div className={`flex flex-row w-full`}>
                            <div className={`flex flex-col flex-1`}>
                                <div
                                    className={`flex flex-row justify-between mb-1.5`}
                                >
                                    <Label
                                        className={`text-[14px] flex justify-center items-center text-blue-600`}
                                    >
                                        <User
                                            className={`size-4 text-primary`}
                                        />
                                        {t("Profile.Title")}
                                    </Label>
                                </div>
                                <div
                                    className={`
                                        flex h-10 w-full cursor-pointer items-center justify-between
                                        rounded-[4px] border-none bg-card px-3
                                    `}
                                >
                                    <span className={`text-[14px]`}>
                                        {activeProfile?.name ??
                                            t("Profile.Default")}
                                    </span>

                                    <Dialog
                                        open={isProfileDialogOpen}
                                        onOpenChange={setIsProfileDialogOpen}
                                    >
                                        <DialogTrigger asChild>
                                            <Button
                                                variant={`ghost`}
                                                className={`p-0 h-6 w-6 rounded-[4px] flex items-center justify-center cursor-pointer`}
                                            >
                                                <Settings
                                                    className={`size-4`}
                                                />
                                            </Button>
                                        </DialogTrigger>

                                        <DialogContent
                                            className={`sm:max-w-[700px] h-[520px] flex flex-col gap-0`}
                                        >
                                            <DialogHeader
                                                className={`flex-none mb-1.5`}
                                            >
                                                <DialogTitle
                                                    className={`!text-[20px] !text-foreground flex-none mb-auto font-sans`}
                                                >
                                                    {t("Profile.Manager")}
                                                </DialogTitle>
                                            </DialogHeader>

                                            <div
                                                className={`flex min-h-0 flex-1 rounded-[4px] border border-border`}
                                            >
                                                <div
                                                    className={`flex w-64 flex-col min-h-0 p-4`}
                                                >
                                                    <div
                                                        className={`text-[14px] flex justify-start mb-1.5 items-center text-blue-600`}
                                                    >
                                                        {t("Profile.List")}
                                                    </div>

                                                    <div
                                                        className={`flex-1 min-h-0 overflow-y-auto space-y-2`}
                                                    >
                                                        {profiles.map(
                                                            (profile) => (
                                                                <button
                                                                    key={
                                                                        profile.name
                                                                    }
                                                                    onClick={() =>
                                                                        setSelectedProfileName(
                                                                            profile.name,
                                                                        )
                                                                    }
                                                                    className={`
                                                                        flex h-10 w-full items-center justify-between rounded-[4px] px-3 transition-colors

                                                                        ${
                                                                            selectedProfile?.name === profile.name
                                                                                ? "bg-primary/10 border border-primary"
                                                                                : "bg-card hover:bg-muted"
                                                                        }
                                                                    `}
                                                                >
                                                                    <span>
                                                                        {
                                                                            profile.name
                                                                        }
                                                                    </span>

                                                                    {profile.isActive && (
                                                                        <span
                                                                            className={`
                                                                                rounded-[4px] bg-green-100
                                                                                px-2 py-0.5 text-xs text-green-700
                                                                            `}
                                                                        >
                                                                            {t(
                                                                                "Profile.Active",
                                                                            )}
                                                                        </span>
                                                                    )}
                                                                </button>
                                                            ),
                                                        )}
                                                    </div>
                                                </div>

                                                <div
                                                    className={`flex flex-1 flex-col bg-muted p-4`}
                                                >
                                                    <div
                                                        className={`text-[14px] flex justify-start mb-1.5 items-center text-blue-600`}
                                                    >
                                                        {t("Profile.Actions")}
                                                    </div>

                                                    <div
                                                        className={`grid grid-cols-1 gap-2`}
                                                    >
                                                        <Dialog
                                                            open={
                                                                isNewProfileDialogOpen
                                                            }
                                                            onOpenChange={
                                                                setIsNewProfileDialogOpen
                                                            }
                                                        >
                                                            <DialogTrigger
                                                                asChild
                                                            >
                                                                <Button
                                                                    className={`justify-start rounded-[4px] text-[14px] bg-card font-normal border border-border text-foreground hover:text-blue-100`}
                                                                    onClick={() => {
                                                                        window.external.sendMessage(
                                                                            `profiles.create`,
                                                                        );
                                                                    }}
                                                                >
                                                                    {t(
                                                                        "Profile.Create",
                                                                    )}
                                                                </Button>
                                                            </DialogTrigger>

                                                            <DialogContent
                                                                className={`sm:max-w-[380px] gap-0`}
                                                            >
                                                                <div
                                                                    className={`space-y-2`}
                                                                >
                                                                    <Label
                                                                        className={`text-[13px]`}
                                                                    >
                                                                        {t(
                                                                            "Profile.Name",
                                                                        )}
                                                                    </Label>

                                                                    <Input
                                                                        value={
                                                                            newProfileName
                                                                        }
                                                                        onChange={(
                                                                            e,
                                                                        ) =>
                                                                            setNewProfileName(
                                                                                e
                                                                                    .target
                                                                                    .value,
                                                                            )
                                                                        }
                                                                        className={`h-7 rounded-[4px] outline-none focus:outline-none focus-visible:ring-0 focus-visible:ring-offset-0`}
                                                                    />
                                                                </div>

                                                                <DialogFooter
                                                                    className={`bg-card flex justify-end mt-auto flex-none border-none`}
                                                                >
                                                                    <Button
                                                                        className={`min-w-20`}
                                                                        onClick={() => {
                                                                            const name =
                                                                                newProfileName.trim();

                                                                            if (
                                                                                !name
                                                                            )
                                                                                return;

                                                                            window.external.sendMessage(
                                                                                `profiles.create|${name}`,
                                                                            );
                                                                            setNewProfileName(
                                                                                ``,
                                                                            );
                                                                            setIsNewProfileDialogOpen(
                                                                                false,
                                                                            );
                                                                        }}
                                                                    >
                                                                        {t(
                                                                            "Common.OK",
                                                                        )}
                                                                    </Button>
                                                                </DialogFooter>
                                                            </DialogContent>
                                                        </Dialog>

                                                        <Dialog
                                                            open={
                                                                isEditProfileDialogOpen
                                                            }
                                                            onOpenChange={
                                                                setIsEditProfileDialogOpen
                                                            }
                                                        >
                                                            <DialogTrigger
                                                                asChild
                                                            >
                                                                <Button
                                                                    className={`justify-start rounded-[4px] text-[14px] bg-card font-normal border border-border text-foreground hover:text-blue-100`}
                                                                    onClick={() => {
                                                                        window.external.sendMessage(
                                                                            `profiles.update`,
                                                                        );
                                                                    }}
                                                                >
                                                                    {t(
                                                                        "Profile.Rename",
                                                                    )}
                                                                </Button>
                                                            </DialogTrigger>

                                                            <DialogContent
                                                                className={`sm:max-w-[380px] gap-0`}
                                                            >
                                                                <div
                                                                    className={`space-y-2`}
                                                                >
                                                                    <Label
                                                                        className={`text-[13px]`}
                                                                    >
                                                                        {t(
                                                                            "Profile.Name",
                                                                        )}
                                                                    </Label>

                                                                    <Input
                                                                        value={
                                                                            editProfileName
                                                                        }
                                                                        onChange={(
                                                                            e,
                                                                        ) =>
                                                                            setEditProfileName(
                                                                                e
                                                                                    .target
                                                                                    .value,
                                                                            )
                                                                        }
                                                                        className={`h-7 rounded-[4px] outline-none focus:outline-none focus-visible:ring-0 focus-visible:ring-offset-0`}
                                                                    />
                                                                </div>

                                                                <DialogFooter
                                                                    className={`bg-card flex justify-end mt-auto flex-none border-none`}
                                                                >
                                                                    <Button
                                                                        className={`min-w-20`}
                                                                        onClick={() => {
                                                                            if (
                                                                                !selectedProfile
                                                                            )
                                                                                return;

                                                                            window.external.sendMessage(
                                                                                `profiles.rename|${selectedProfile.name}|${editProfileName.trim()}`,
                                                                            );

                                                                            setEditProfileName(
                                                                                ``,
                                                                            );
                                                                            setIsEditProfileDialogOpen(
                                                                                false,
                                                                            );
                                                                        }}
                                                                    >
                                                                        {t(
                                                                            "Common.OK",
                                                                        )}
                                                                    </Button>
                                                                </DialogFooter>
                                                            </DialogContent>
                                                        </Dialog>

                                                        <Button
                                                            className={`justify-start rounded-[4px] text-[14px] bg-card font-normal border border-border text-foreground hover:text-blue-100`}
                                                            onClick={() => {
                                                                if (
                                                                    !selectedProfile
                                                                )
                                                                    return;

                                                                setLockMessage(
                                                                    "Lock.BackingUpProfile",
                                                                );
                                                                setIsLocked(
                                                                    true,
                                                                );

                                                                window.external.sendMessage(
                                                                    `profiles.backup|${selectedProfile.name}`,
                                                                );
                                                            }}
                                                        >
                                                            {t(
                                                                "Profile.Backup",
                                                            )}
                                                        </Button>

                                                        <Button
                                                            className={`justify-start rounded-[4px] text-[14px] bg-card font-normal border border-border text-foreground hover:text-blue-100`}
                                                            onClick={() => {
                                                                if (
                                                                    !selectedProfile
                                                                )
                                                                    return;

                                                                setLockMessage(
                                                                    "Lock.ExportingProfile",
                                                                );
                                                                setIsLocked(
                                                                    true,
                                                                );

                                                                window.external.sendMessage(
                                                                    `profiles.export|${selectedProfile.name}`,
                                                                );
                                                            }}
                                                        >
                                                            {t(
                                                                "Profile.Export",
                                                            )}
                                                        </Button>
                                                    </div>
                                                </div>
                                            </div>

                                            <DialogFooter
                                                className={`bg-card flex justify-end mt-auto flex-none border-none`}
                                            >
                                                <Button
                                                    className={`min-w-20`}
                                                    onClick={() => {
                                                        if (!selectedProfile)
                                                            return;

                                                        window.external.sendMessage(
                                                            `profiles.switch|${selectedProfile.name}`,
                                                        );

                                                        setIsProfileDialogOpen(
                                                            false,
                                                        );
                                                    }}
                                                >
                                                    {t("Common.Apply")}
                                                </Button>
                                            </DialogFooter>
                                        </DialogContent>
                                    </Dialog>
                                </div>
                            </div>
                        </div>
                    </div>

                    <div
                        className={`rounded-[4px] bg-muted flex flex-col p-4`}
                    >
                        <div className={`flex flex-row justify-between mb-1.5`}>
                            <Label
                                className={`text-[14px] flex justify-center items-center text-blue-600`}
                            >
                                <HeartPulse className={`size-4 text-primary`} />
                                {t("Check.Title")}
                            </Label>
                        </div>
                        <div
                            className={`flex flex-col justify-between bg-card p-2`}
                        >
                            {warningCount > 0 && (
                                <div
                                    className={`flex flex-row justify-between items-center`}
                                >
                                    <Label
                                        className={`text-[14px] font-normal`}
                                    >
                                        {t("Mod.Status.Warning")}
                                    </Label>
                                    <Label
                                        className={`text-[14px] font-medium`}
                                    >
                                        {warningCount > 0 && (
                                            <Badge
                                                className={`
                                                    bg-yellow-100
                                                    text-yellow-700
                                                `}
                                            >
                                                {warningCount}
                                            </Badge>
                                        )}
                                    </Label>
                                </div>
                            )}
                            {errorCount > 0 && (
                                <div
                                    className={`flex flex-row justify-between items-center`}
                                >
                                    <Label
                                        className={`text-[14px] font-normal`}
                                    >
                                        {t("Mod.Status.Error")}
                                    </Label>
                                    <Label
                                        className={`text-[14px] font-medium`}
                                    >
                                        {errorCount > 0 && (
                                            <Badge variant={`destructive`}>
                                                {errorCount}
                                            </Badge>
                                        )}
                                    </Label>
                                </div>
                            )}
                        </div>
                    </div>

                    <div
                        className={`rounded-[4px] bg-muted flex flex-col p-4 flex-1`}
                    >
                        <div className={`flex flex-row justify-between mb-1.5`}>
                            <Label
                                className={`text-[14px] flex justify-center items-center text-blue-600`}
                            >
                                <NotepadText
                                    className={`size-4 text-primary`}
                                />
                                {t("Note.Title")}
                            </Label>
                        </div>
                        <div className={`bg-background flex-1 flex justify-center items-center`}>Comming Soon</div>
                    </div>

                    <div
                        className={`rounded-[4px] bg-muted flex flex-row space-x-4 p-4`}
                    >
                        <div
                            className={`w-1/2 flex flex-row items-center justify-start gap-4 hover:text-primary cursor-pointer`}
                            onClick={() => {
                                window.external.sendMessage("app.openNexus");
                            }}
                        >
                            <SquareArrowOutUpLeft
                                className={`size-4.5 text-foreground`}
                            />
                            <Label className={`text-[14px] font-medium`}>
                                {t("Link.Nexus")}
                            </Label>
                        </div>
                        <Separator
                            orientation={`vertical`}
                            className={`hidden md:block bg-border`}
                        />
                        <div
                            className={`w-1/2 flex flex-row items-center justify-start gap-4 hover:text-primary cursor-pointer`}
                            onClick={() => {
                                window.external.sendMessage("app.openKofi");
                            }}
                        >
                            <Coffee className={`size-4.5 text-foreground`} />
                            <Label className={`text-[14px] font-medium`}>
                                {t("Link.Kofi")}
                            </Label>
                        </div>
                    </div>

                    <div
                        className={`flex flex-row text-xs text-muted-foreground justify-end`}
                    >
                        <Label>{t("App.Version", { version })}</Label>
                    </div>
                </div>
            </div>
        </>
    );
}

export default App;
