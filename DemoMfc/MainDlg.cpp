#include "stdafx.h"
#include "MainDlg.h"
#include "GenMotionEasyCpp/GtnError.h"

BEGIN_MESSAGE_MAP(CMainDlg, CDialogEx)
    ON_BN_CLICKED(IDC_BTN_OPEN, OnBtnOpen)
    ON_BN_CLICKED(IDC_BTN_CLOSE, OnBtnClose)
    ON_BN_CLICKED(IDC_BTN_ECAT_LOAD, OnBtnEcatLoad)
    ON_BN_CLICKED(IDC_BTN_ECAT_START, OnBtnEcatStart)
    ON_BN_CLICKED(IDC_BTN_AXIS_ON, OnBtnAxisOn)
    ON_BN_CLICKED(IDC_BTN_AXIS_OFF, OnBtnAxisOff)
    ON_BN_CLICKED(IDC_BTN_JOG_P, OnBtnJogP)
    ON_BN_CLICKED(IDC_BTN_JOG_N, OnBtnJogN)
    ON_BN_CLICKED(IDC_BTN_STOP, OnBtnStop)
    ON_BN_CLICKED(IDC_BTN_POINT_MOVE, OnBtnPointMove)
    ON_CBN_SELCHANGE(IDC_CMB_AXIS, OnSelChangeAxis)
    ON_WM_TIMER()
END_MESSAGE_MAP()

CMainDlg::CMainDlg(CWnd* pParent)
    : CDialogEx(IDD_MAIN_DIALOG, pParent)
    , _mgr(nullptr), _axis(nullptr), _curAxis(1), _timerId(0) {}

void CMainDlg::DoDataExchange(CDataExchange* pDX) {
    CDialogEx::DoDataExchange(pDX);
    DDX_Control(pDX, IDC_CMB_AXIS, _cmbAxis);
    DDX_Control(pDX, IDC_EDIT_VEL, _edtVel);
    DDX_Control(pDX, IDC_EDIT_ACC, _edtAcc);
    DDX_Control(pDX, IDC_EDIT_DEC, _edtDec);
    DDX_Control(pDX, IDC_EDIT_POS, _edtPos);
    DDX_Control(pDX, IDC_ST_STATUS, _stStatus);
}

BOOL CMainDlg::OnInitDialog() {
    CDialogEx::OnInitDialog();
    SetWindowText(_T("GenMotion MFC Demo"));

    for (int i = 1; i <= 8; i++) {
        CString s; s.Format(_T("Axis %d"), i);
        _cmbAxis.AddString(s);
    }
    _cmbAxis.SetCurSel(0);

    _edtVel.SetWindowText(_T("50000"));
    _edtAcc.SetWindowText(_T("500000"));
    _edtDec.SetWindowText(_T("500000"));
    _edtPos.SetWindowText(_T("100000"));

    SetDlgItemText(IDC_ST_STATUS, _T("Ready"));
    _timerId = SetTimer(1, 300, nullptr);
    return TRUE;
}

void CMainDlg::UpdateAxis() {
    if (!_mgr) { _axis = nullptr; return; }
    try {
        _axis = _mgr->GetAxisController(_curAxis);
    } catch (...) {
        _axis = nullptr;
    }
}

void CMainDlg::OnBtnOpen() {
    try {
        _mgr = new MotionControlManager();
        if (_mgr->Open()) {
            _mgr->AddAxis(1); _mgr->AddAxis(2); _mgr->AddAxis(3);
            UpdateAxis();
            SetDlgItemText(IDC_ST_STATUS, _T("Opened"));
        } else {
            SetDlgItemText(IDC_ST_STATUS, _T("Open failed"));
        }
    } catch (GtnException& e) {
        CString s; s.Format(_T("Error: %hs"), e.what());
        SetDlgItemText(IDC_ST_STATUS, s);
    }
}

void CMainDlg::OnBtnClose() {
    if (_mgr) { _mgr->Close(); delete _mgr; _mgr = nullptr; _axis = nullptr; }
    SetDlgItemText(IDC_ST_STATUS, _T("Closed"));
}

void CMainDlg::OnBtnEcatLoad() {
    if (!_mgr) return;
    short rtn = _mgr->EcatLoad();
    CString s; s.Format(_T("EcatLoad: %d"), rtn);
    SetDlgItemText(IDC_ST_STATUS, s);
}

void CMainDlg::OnBtnEcatStart() {
    if (!_mgr) return;
    short rtn = _mgr->EcatStart();
    CString s; s.Format(_T("EcatStart: %d"), rtn);
    SetDlgItemText(IDC_ST_STATUS, s);
}

void CMainDlg::OnBtnAxisOn() {
    if (!_axis) return;
    short rtn = _axis->EnableAxis();
    CString s; s.Format(_T("AxisOn: %d"), rtn);
    SetDlgItemText(IDC_ST_STATUS, s);
}

void CMainDlg::OnBtnAxisOff() {
    if (!_axis) return;
    short rtn = _axis->DisableAxis();
    CString s; s.Format(_T("AxisOff: %d"), rtn);
    SetDlgItemText(IDC_ST_STATUS, s);
}

void CMainDlg::OnBtnJogP() {
    if (!_axis) return;
    CString sv; _edtVel.GetWindowText(sv);
    double vel = _tstof(sv);
    CString sa; _edtAcc.GetWindowText(sa);
    double acc = _tstof(sa);
    CString sd; _edtDec.GetWindowText(sd);
    double dec = _tstof(sd);
    try {
        _axis->AxisHome(); // dummy call to validate axis
        IJogMotion* jog = _axis->Jog();
        if (jog) {
            jog->SetJogMode(acc, dec);
            jog->JogMove(vel);
        }
        SetDlgItemText(IDC_ST_STATUS, _T("Jog+"));
    } catch (GtnException& e) {
        CString s; s.Format(_T("Jog+ Error: %hs"), e.what());
        SetDlgItemText(IDC_ST_STATUS, s);
    }
}

void CMainDlg::OnBtnJogN() {
    if (!_axis) return;
    CString sv; _edtVel.GetWindowText(sv);
    double vel = -_tstof(sv);
    CString sa; _edtAcc.GetWindowText(sa);
    double acc = _tstof(sa);
    CString sd; _edtDec.GetWindowText(sd);
    double dec = _tstof(sd);
    try {
        IJogMotion* jog = _axis->Jog();
        if (jog) {
            jog->SetJogMode(acc, dec);
            jog->JogMove(vel);
        }
        SetDlgItemText(IDC_ST_STATUS, _T("Jog-"));
    } catch (GtnException& e) {
        CString s; s.Format(_T("Jog- Error: %hs"), e.what());
        SetDlgItemText(IDC_ST_STATUS, s);
    }
}

void CMainDlg::OnBtnStop() {
    if (!_axis) return;
    _axis->StopAxis();
    SetDlgItemText(IDC_ST_STATUS, _T("Stopped"));
}

void CMainDlg::OnBtnPointMove() {
    if (!_axis) return;
    CString sp; _edtPos.GetWindowText(sp);
    long pos = _ttol(sp);
    CString sv; _edtVel.GetWindowText(sv);
    double vel = _tstof(sv);
    CString sa; _edtAcc.GetWindowText(sa);
    double acc = _tstof(sa);
    CString sd; _edtDec.GetWindowText(sd);
    double dec = _tstof(sd);
    try {
        IPointMotion* pt = _axis->Point();
        if (pt) pt->PointAbsMove(pos, vel, acc, dec);
        SetDlgItemText(IDC_ST_STATUS, _T("PointMove OK"));
    } catch (GtnException& e) {
        CString s; s.Format(_T("PointMove Error: %hs"), e.what());
        SetDlgItemText(IDC_ST_STATUS, s);
    }
}

void CMainDlg::OnSelChangeAxis() {
    _curAxis = _cmbAxis.GetCurSel() + 1;
    UpdateAxis();
}

void CMainDlg::OnTimer(UINT_PTR id) {
    if (!_axis) return;
    StatusInfo si = _axis->GetEcatStatus();
    CString s;
    s.Format(_T("Pos:%.0f  Vel:%.0f  Enc:%.0f  En:%d  Alm:%d  Stop:%d"),
        si.plannedLocation, si.plannedVel, si.driveLocation,
        si.enableAxis, si.axisAlarm, si.smoothStopAlarm);
    SetDlgItemText(IDC_ST_STATUS, s);
}
