// AutoDL2Wrapper.h

#pragma once
#pragma comment(lib, "user32.lib")
#pragma comment(linker, "/EXPORT:LoadDll=_LoadDll@4")
#pragma comment(linker, "/EXPORT:UnloadDll=_UnloadDll@4")
#pragma comment(linker, "/EXPORT:AutoDL_Start=_AutoDL_Start@24")
#pragma comment(linker, "/EXPORT:RequestNextDownload=_RequestNextDownload@24")

using namespace System;
using namespace System::Reflection;
using namespace AutoDL;
using namespace AutoDL::ServiceContracts;

#define MIRC_BUFFER 4096
#define MIRC_DATABUFFER 900
#define MIRC_FILEMAP L"mIRC7"
#define MIRC_FILEMAPNUM 7
#define WM_MCOMMAND WM_USER + 200
#define WM_MEVALUATE WM_USER + 201
#define SERVICE_EXTENSION "mIRC"
#define mIRCFunc(x) int __stdcall x(HWND mWnd, HWND aWnd, char* data, char* params, bool show, bool nopause)

typedef struct {
	DWORD  mVersion;
	HWND   mHwnd;
	BOOL   mKeep;
	BOOL   mUnicode;
} LOADINFO;

extern "C"
{
	void __stdcall LoadDll(LOADINFO*);
	int __stdcall UnloadDll(int);
	mIRCFunc(AutoDL_Start);
	mIRCFunc(RequestNextDownload);
}
