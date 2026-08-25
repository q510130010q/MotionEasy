#pragma once
#include "resource.h"
#include "GenMotionEasyCpp/MotionControlManager.h"

class CMainDlg : public CDialogEx {
public:
    CMainDlg(CWnd* pParent = nullptr);
    enum { IDD = IDD_MAIN_DIALOG };

private:
    MotionControlManager* _mgr;
    AxisController* _axis;
    short _curAxis;

    CComboBox _cmbAxis;
    CEdit _edtVel, _edtAcc, _edtDec, _edtPos;
    CStatic _stStatus;
    UINT_PTR _timerId;

    virtual void DoDataExchange(CDataExchange* pDX);
    virtual BOOL OnInitDialog();
    afx_msg void OnBtnOpen();
    afx_msg void OnBtnClose();
    afx_msg void OnBtnEcatLoad();
    afx_msg void OnBtnEcatStart();
    afx_msg void OnBtnAxisOn();
    afx_msg void OnBtnAxisOff();
    afx_msg void OnBtnJogP();
    afx_msg void OnBtnJogN();
    afx_msg void OnBtnStop();
    afx_msg void OnBtnPointMove();
    afx_msg void OnSelChangeAxis();
    afx_msg void OnTimer(UINT_PTR id);
    void UpdateAxis();

    DECLARE_MESSAGE_MAP()
};
