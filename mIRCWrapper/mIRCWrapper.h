// AutoDL2Wrapper.h

#pragma once
#pragma comment(linker, "/EXPORT:LoadDll=_LoadDll@4")
#pragma comment(linker, "/EXPORT:UnloadDll=_UnloadDll@4")

using namespace System;
using namespace AutoDL;

#define MIRC_MAXBUFFER 4096
#define WM_MCOMMAND WM_USER + 200
#define ITEM_SEPARATOR ","
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
}
