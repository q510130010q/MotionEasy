#include "stdafx.h"
#include "DemoMfc.h"
#include "MainDlg.h"

BEGIN_MESSAGE_MAP(CDemoMfcApp, CWinApp)
END_MESSAGE_MAP()

CDemoMfcApp theApp;

BOOL CDemoMfcApp::InitInstance() {
    INITCOMMONCONTROLSEX icc = { sizeof(INITCOMMONCONTROLSEX), ICC_WIN95_CLASSES };
    InitCommonControlsEx(&icc);
    CWinApp::InitInstance();
    CMainDlg dlg;
    m_pMainWnd = &dlg;
    dlg.DoModal();
    return FALSE;
}
