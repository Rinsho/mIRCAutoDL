// AutoDLWrapper.h

#pragma once
#pragma comment(linker, "/EXPORT:LoadDll=_LoadDll@4")
#pragma comment(linker, "/EXPORT:UnloadDll=_UnloadDll@4")
#pragma comment(linker, "/EXPORT:Queue_Add=_Queue_Add@24")
#pragma comment(linker, "/EXPORT:Queue_Remove=_Queue_Remove@24")
#pragma comment(linker, "/EXPORT:Queue_Clear=_Queue_Clear@24")
#pragma comment(linker, "/EXPORT:Queue_NextItem=_Queue_NextItem@24")
#pragma comment(linker, "/EXPORT:Queue_Save=_Queue_Save@24")
#pragma comment(linker, "/EXPORT:Queue_Load=_Queue_Load@24")
#pragma comment(linker, "/EXPORT:Queue_ClearSaved=_Queue_ClearSaved@24")
#pragma comment(linker, "/EXPORT:Settings_Save=_Settings_Save@24")
#pragma comment(linker, "/EXPORT:Settings_Load=_Settings_Load@24")
#pragma comment(linker, "/EXPORT:Nick_Add=_Nick_Add@24")
#pragma comment(linker, "/EXPORT:Nick_Remove=_Nick_Remove@24")
#pragma comment(linker, "/EXPORT:Nick_Clear=_Nick_Clear@24")
#pragma comment(linker, "/EXPORT:Nick_GetName=_Nick_GetName@24")
#pragma comment(linker, "/EXPORT:Nick_GetAll=_Nick_GetAll@24")

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

	mIRCFunc(Queue_Add);
	mIRCFunc(Queue_Remove);
	mIRCFunc(Queue_Clear);
	mIRCFunc(Queue_NextItem);
	mIRCFunc(Queue_Save);
	mIRCFunc(Queue_Load);
	mIRCFunc(Queue_ClearSaved);
	mIRCFunc(Settings_Save);
	mIRCFunc(Settings_Load);
	mIRCFunc(Nick_Add);
	mIRCFunc(Nick_Remove);
	mIRCFunc(Nick_Clear);
	mIRCFunc(Nick_GetName);
	mIRCFunc(Nick_GetAll);
}