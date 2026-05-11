using System.Drawing;
using System.Windows.Forms;

namespace OnTopReplica.Tests {

    /// <summary>
    /// Documents the ResizeLock contract: locking pins the size; unlocking restores
    /// the pre-lock Min/Max (not zero). Implemented behaviorally against a stand-in Form
    /// because MainForm requires the full Program.Platform bootstrap to instantiate.
    /// </summary>
    public class ResizeLockTests {

        class Lockable : Form {
            bool _locked;
            Size _preMin, _preMax;
            public bool ResizeLockEnabled {
                get { return _locked; }
                set {
                    if (value == _locked) return;
                    if (value) { _preMin = MinimumSize; _preMax = MaximumSize; MaximumSize = Size; MinimumSize = Size; }
                    else { MaximumSize = _preMax; MinimumSize = _preMin; }
                    _locked = value;
                }
            }
        }

        [Test]
        public void Lock_PinsMinAndMaxToCurrentSize() {
            using (var f = new Lockable()) {
                f.Size = new Size(400, 300);
                f.ResizeLockEnabled = true;
                Assert.AreEqual(new Size(400, 300), f.MinimumSize);
                Assert.AreEqual(new Size(400, 300), f.MaximumSize);
            }
        }

        [Test]
        public void Unlock_RestoresPreLockMinMax() {
            using (var f = new Lockable()) {
                f.MinimumSize = new Size(200, 150);
                f.MaximumSize = new Size(800, 600);
                f.Size = new Size(400, 300);

                f.ResizeLockEnabled = true;
                f.ResizeLockEnabled = false;

                Assert.AreEqual(new Size(200, 150), f.MinimumSize);
                Assert.AreEqual(new Size(800, 600), f.MaximumSize);
            }
        }

        [Test]
        public void Unlock_WithNoPriorConstraints_RestoresEmpty() {
            using (var f = new Lockable()) {
                f.Size = new Size(400, 300);
                f.ResizeLockEnabled = true;
                f.ResizeLockEnabled = false;

                Assert.AreEqual(Size.Empty, f.MinimumSize);
                Assert.AreEqual(Size.Empty, f.MaximumSize);
            }
        }

        [Test]
        public void RedundantSetSameValue_IsNoOp() {
            using (var f = new Lockable()) {
                f.MinimumSize = new Size(100, 100);
                f.Size = new Size(400, 300);
                f.ResizeLockEnabled = true;
                // Setting again must not overwrite saved pre-lock state with locked values.
                f.ResizeLockEnabled = true;
                f.ResizeLockEnabled = false;
                Assert.AreEqual(new Size(100, 100), f.MinimumSize);
            }
        }
    }
}
