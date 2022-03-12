/*******************************************************************************
 * Copyright (c) 2016, George Sedov
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice,
 * this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 * this list of conditions and the following disclaimer in the documentation
 * and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 ******************************************************************************/

using UnityEngine;
using UnityEngine.UI;

namespace KSPPreciseManeuver.UI {
  [RequireComponent (typeof (RectTransform))]
  public class GizmoControl : MonoBehaviour {
    [SerializeField]
    private Button m_UndoButton = null;
    [SerializeField]
    private Button m_RedoButton = null;
    [SerializeField]
    private Slider m_SensitivitySlider = null;
    [SerializeField]
    private Button m_APButton = null;
    [SerializeField]
    private Button m_PEButton = null;
    [SerializeField]
    private Button m_POButton = null;
    [SerializeField]
    private Button m_MOButton = null;

    private IGizmoControl m_Control = null;

    private double ddx = 0, ddy = 0, ddz = 0, dut = 0;
    private bool nonzero = false;

    public void SetControl (IGizmoControl control) {
      m_Control = control;
      m_SensitivitySlider.value = m_Control.Sensitivity;
      UpdateGUI ();
      m_Control.RegisterUpdateAction (UpdateGUI);
    }

    public void OnDestroy () {
      m_Control.DeregisterUpdateAction (UpdateGUI);
      m_Control = null;
    }

    private bool dragging;
    public bool Dragging {
      get { return dragging; }
      set {
        dragging = value;
        if (dragging) {
          //begin drag, save the node for undo
          m_Control.BeginAtomicChange ();
        } else {
          m_Control.EndAtomicChange ();
          //end drag, notify the elements that they can glow now
          foreach (var gizmoElement in GetComponentsInChildren<GizmoElement> ())
            gizmoElement.GlowOn ();
        }
      }
    }

    public void UndoAction () {
      m_Control.Undo ();
    }

    public void RedoAction () {
      m_Control.Redo ();
    }

    public void SensitivityChange (float val) {
      m_Control.Sensitivity = val;
    }
    public void APButtonAction () {
      m_Control.APButtonPressed ();
    }
    public void PEButtonAction () {
      m_Control.PEButtonPressed ();
    }
    public void POButtonAction () {
      m_Control.POButtonPressed ();
    }
    public void MOButtonAction () {
      m_Control.MOButtonPressed ();
    }
    public void RepeatButtonStart () {
      m_Control.BeginAtomicChange ();
    }
    public void RepeatButtonStop () {
      m_Control.EndAtomicChange ();
    }
    public void ChangeddV (double ddx, double ddy, double ddz, double dut) {
      double scale = Mathf.Pow (10, m_SensitivitySlider.value);

      this.ddx = ddx * scale;
      this.ddy = ddy * scale;
      this.ddz = ddz * scale;
      this.dut = dut * scale;
      nonzero = (ddx != 0.0) || (ddy != 0.0) || (ddz != 0.0) || (dut != 0.0);
    }

    public void FixedUpdate () {
      if (nonzero)
        m_Control.UpdateNode (ddx, ddy, ddz, dut);
    }
    public void UpdateGUI () {
      if (m_Control.UndoAvailable) {
        m_UndoButton.interactable = true;
        m_UndoButton.GetComponent<Image> ().color = new Color (1.0f, 1.0f, 1.0f, 1.0f);
      } else {
        m_UndoButton.interactable = false;
        m_UndoButton.GetComponent<Image> ().color = new Color (0.0f, 0.0f, 0.0f, 0.25f);
      }
      if (m_Control.RedoAvailable) {
        m_RedoButton.interactable = true;
        m_RedoButton.GetComponent<Image> ().color = new Color (1.0f, 1.0f, 1.0f, 1.0f);
      } else {
        m_RedoButton.interactable = false;
        m_RedoButton.GetComponent<Image> ().color = new Color (0.0f, 0.0f, 0.0f, 0.25f);
      }
      m_APButton.interactable = m_Control.APAvailable;
      m_PEButton.interactable = m_Control.PEAvailable;
      m_POButton.interactable = m_Control.POAvailable;
      m_MOButton.interactable = m_Control.MOAvailable;
    }
  }
}
