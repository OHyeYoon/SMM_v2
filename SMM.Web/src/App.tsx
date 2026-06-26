import { BookUser, CircleCheck, FolderOpen, HeartPulse, NotepadText, Play, Users } from "lucide-react";
import "./App.css";
import { Button } from "./components/ui/button";
import {
    Card,
    CardAction,
    CardContent,
    CardDescription,
    CardHeader,
    CardTitle,
} from "./components/ui/card";
import { Label } from "./components/ui/label";

function App() {
    return (
        <>
            <div className={`flex flex-row p-4 w-full h-full`}>
                <div className={`flex-1 flex flex-col`}>
                    <div className={`flex flex-row space-x-4`}>
                        <Card className={`w-1/3 h-20 p-4 rounded-2xl flex flex-col justify-start`}>
                            <CardHeader className={`flex flex-row p-0 h-full`}>
                                <CardTitle className={`flex flex-row items-center justify-center mr-2`}>
                                    <CircleCheck className={`text-blue-500 size-8`}/>
                                </CardTitle>
                                <CardDescription className={`flex flex-col flex-1 gap-0.5`}>
                                    <Label className={`text-[16px] text-black font-semibold`}>SMAPI</Label>
                                    <Label className={`text-[14px] text-green-800 font-medium`}>Installed</Label>
                                    <Label className={`text-[12px]`}>4.3.5</Label>
                                </CardDescription>
                                <CardAction className={`flex items-start`}>
                                    <FolderOpen className={`size-4`}/>
                                </CardAction>
                            </CardHeader>
                        </Card>
                        <Card className={`w-1/3 h-20 p-4 rounded-2xl flex flex-col justify-start`}>
                            <CardHeader className={`flex flex-row p-0 h-full`}>
                                <CardTitle className={`flex flex-row items-center justify-center mr-2`}>
                                    <CircleCheck className={`text-blue-500 size-8`}/>
                                </CardTitle>
                                <CardDescription className={`flex flex-col flex-1 gap-1`}>
                                    <Label className={`text-[16px] text-black font-semibold`}>StardewVally</Label>
                                    <Label className={`text-[12px]`}>4.3.5</Label>
                                </CardDescription>
                                <CardAction className={`flex items-start`}>
                                    <FolderOpen className={`size-4`}/>
                                </CardAction>
                            </CardHeader>
                        </Card>
                        <Card className={`w-1/3 h-20 p-4 rounded-2xl flex flex-col justify-start`}>
                            <CardHeader className={`flex flex-row p-0 h-full`}>
                                <CardTitle className={`flex flex-row items-center justify-center mr-2`}>
                                    <Users className={`text-blue-500 size-8`}/>
                                </CardTitle>
                                <CardDescription className={`flex flex-col flex-1 gap-1`}>
                                    <Label className={`text-[16px] text-black font-semibold`}>Default</Label>
                                    <Label className={`text-[12px]`}>Last played 2026-06-26 08:10:00</Label>
                                </CardDescription>
                            </CardHeader>
                        </Card>
                    </div>
                    <div className={`flex-1`}>모드리스트</div>
                </div>
                <div className={`flex flex-col w-100 space-y-4 ml-4`}>
                    <Button className={`h-20 font-bold text-2xl flex items-center justify-center`}><Play className={`size-6 mr-2`}/>START</Button>
                    <Card>
                        <CardHeader>
                            <CardTitle className={`flex flex-row gap-2 items-center`}>
                                <HeartPulse size={`18`}/>
                                <label className={`text-sm`}>Check</label>
                            </CardTitle>
                        </CardHeader>
                        <CardContent className={`flex- flex-col space-y-2`}>
                            <Label>SMAPI installed</Label>
                            <Label>Game version check</Label>
                            <Label>Warning 10</Label>
                            <Label>Error 10</Label>
                        </CardContent>
                    </Card>
                    <Card>
                        <CardHeader>
                            <CardTitle className={`flex flex-row gap-2 items-center`}>
                                <BookUser size={`18`}/>
                                <label className={`text-sm`}>Profile Note</label>
                            </CardTitle>
                        </CardHeader>
                        <CardContent className={`flex- flex-col space-y-2`}>
                            <Label>SMAPI installed</Label>
                            <Label>Game version check</Label>
                            <Label>Warning 10</Label>
                            <Label>Error 10</Label>
                        </CardContent>
                    </Card>
                    <Card>
                        <CardHeader>
                            <CardTitle className={`flex flex-row gap-2 items-center`}>
                                <NotepadText size={`18`}/>
                                <label className={`text-sm`}>Mod Note</label>
                            </CardTitle>
                        </CardHeader>
                        <CardContent className={`flex- flex-col space-y-2`}>
                            <Label>SMAPI installed</Label>
                            <Label>Game version check</Label>
                            <Label>Warning 10</Label>
                            <Label>Error 10</Label>
                        </CardContent>
                    </Card>
                </div>
            </div>
        </>
    );
}

export default App;
