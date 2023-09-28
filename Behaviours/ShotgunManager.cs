using System.Collections;
using UnityEngine;
using UnityEngine.XR;
using HoneyLib.Utils;

namespace GorillaShotgun.Behaviours
{
    class ShotgunManager : MonoBehaviour
    {
        internal bool modeLock = false;
        internal bool fireLock = false;
        internal bool downLock = false;
        internal bool upLock = true;
        internal bool pullLock = true;
        internal bool leftFired;
        public bool chainFired;
        internal bool chainState;
        internal int ammo = 2;
        public bool doingSomething { get; internal set; }
        public static ShotgunManager instance = null;
        ParticleSystem[] particlesL;
        ParticleSystem[] particlesR;
        Material[] full;
        Material[] empty;
        AudioSource audio;
        Animation animator;
        Animation spinAnimator;
        GameObject shellL;
        GameObject shellR;
        GameObject aim;
        Transform aimPoint;
        public LineRenderer chain;
        Vector3 controllerVelocity;
        public Vector3 chainHitPos;
        Coroutine modeRoutine;
        Coroutine fireRoutine;
        Coroutine upRoutine;
        Coroutine downRoutine;

        void Awake()
        {
            instance = this;
            audio = GetComponent<AudioSource>();
            animator = transform.GetChild(0).GetChild(0).GetComponent<Animation>();
            particlesL = animator.transform.GetChild(0).GetChild(0).GetComponentsInChildren<ParticleSystem>();
            particlesR = animator.transform.GetChild(0).GetChild(1).GetComponentsInChildren<ParticleSystem>();
            shellL = animator.transform.GetChild(1).GetChild(0).gameObject;
            shellR = animator.transform.GetChild(1).GetChild(1).gameObject;
            shellL.layer = 8;
            shellR.layer = 8;
            full = shellL.GetComponent<Renderer>().materials;
            empty = Plugin.emptyRenderer.materials;
            spinAnimator = GetComponent<Animation>(); ;
            aim = animator.transform.GetChild(0).GetChild(2).GetChild(0).gameObject;
            aimPoint = animator.transform.GetChild(0);
            chain = animator.transform.GetChild(0).GetChild(2).GetChild(1).GetComponent<LineRenderer>();
            UpdateInfo();
        }

        void FixedUpdate()
        {
            doingSomething = fireRoutine != null || upRoutine != null || downRoutine != null || modeRoutine != null;

            InputDevices.GetDeviceAtXRNode(Config.ShotgunConfig.isLeft.Value ? XRNode.LeftHand : XRNode.RightHand).TryGetFeatureValue(CommonUsages.deviceVelocity, out controllerVelocity);

            if (Config.ShotgunConfig.isLeft.Value ? EasyInput.LeftTrigger : EasyInput.RightTrigger)
            {
                if (!fireLock && ammo > 0 && !animator.isPlaying)
                {
                    fireRoutine = StartCoroutine(Fire());
                }
            }

            if(controllerVelocity.y < -1.5f && controllerVelocity.y > -2.75f)
            {
                if (!downLock && ammo == 0)
                {
                    downRoutine = StartCoroutine(FlickDown());
                }
            }
            if (controllerVelocity.y > 1.5f && controllerVelocity.y < 2.75f)
            {
                if(!upLock && ammo == 0)
                {
                    upRoutine = StartCoroutine(FlickUp());
                }
            }

            if (Config.ShotgunConfig.isLeft.Value ? EasyInput.FaceButtonX : EasyInput.FaceButtonA)
            {
                if (!chainFired)
                {
                    if (!modeLock && ammo != 0) modeRoutine = StartCoroutine(ToggleMode());
                }
                else
                {
                    chainFired = false;
                    chain.SetPositions(new Vector3[] { Vector3.zero, Vector3.zero });
                    aim.SetActive(true);
                    fireLock = false;
                }
            }
            else if (chainFired)
            {
                chainFired = true;
                if (controllerVelocity.magnitude > 5.8f)
                {
                    if (!pullLock)
                    {
                        GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.AddForce(((chainHitPos - GorillaLocomotion.Player.Instance.bodyCollider.transform.position) *
                            Config.ShotgunConfig.force.Value * controllerVelocity.magnitude) / 2.25f, ForceMode.VelocityChange);
                        pullLock = true;
                    }
                }
                else pullLock = false;
                chain.SetPositions(new Vector3[] { chain.transform.position, chainHitPos });
            }
            else
            {
                chain.SetPositions(new Vector3[] { Vector3.zero, Vector3.zero });
            }
        }

        IEnumerator ToggleMode()
        {
            doingSomething = true;
            modeLock = true;
            spinAnimator.Play("spin");
            yield return new WaitForSeconds(0.25f);
            chainState = !chainState;
            chain.SetPositions(new Vector3[] { Vector3.zero, Vector3.zero });
            aim.SetActive(chainState);
            modeLock = false;
            doingSomething = false;
            modeRoutine = null;
            yield break;
        }

        IEnumerator Fire()
        {
            RaycastHit hit;
            if (!chainState)
            {
                doingSomething = true;
                fireLock = true;
                ammo--;
                audio.PlayOneShot(Plugin.fire);
                if (!leftFired)
                {
                    for (int i = 0; i < particlesL.Length; i++)
                    {
                        particlesL[i].Play();
                    }
                    shellL.GetComponent<Renderer>().materials = empty;
                    leftFired = true;
                }
                else
                {
                    for (int i = 0; i < particlesR.Length; i++)
                    {
                        particlesR[i].Play();
                    }
                    shellR.GetComponent<Renderer>().materials = empty;
                }
                if (Plugin.inRoom)
                {
                    GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.AddForce(transform.up * 35000f * Config.ShotgunConfig.force.Value, ForceMode.Force);
                }
                yield return new WaitForSeconds(0.45f);
                fireLock = false;
                fireRoutine = null;
            }
            else if (Plugin.inRoom && Physics.Raycast(chain.transform.position, -chain.transform.right, out hit))
            {
                doingSomething = true;
                fireLock = true;
                aim.SetActive(false);
                chainHitPos = hit.point;
                chainFired = true;
                fireRoutine = null;
                yield break;
            }
        }

        IEnumerator FlickUp()
        {
            doingSomething = true;
            upLock = true;
            animator.Play("barrelclose");
            audio.PlayOneShot(Plugin.load);
            yield return new WaitForSeconds(0.2f);
            shellL.GetComponent<Renderer>().enabled = true;
            shellL.GetComponent<Renderer>().materials = full;
            shellR.GetComponent<Renderer>().enabled = true;
            shellR.GetComponent<Renderer>().materials = full;
            yield return new WaitForSeconds(0.05f);
            ammo = 2;
            leftFired = false;
            downLock = false;
            upRoutine = null;
        }

        IEnumerator FlickDown()
        {
            doingSomething = true;
            downLock = true;
            ammo = 0;
            animator.Play("barrelopen");
            audio.PlayOneShot(Plugin.load);

            CreateNewShell(shellL);
            CreateNewShell(shellR);

            yield return new WaitForSeconds(0.25f);

            upLock = false;
            downRoutine = null;
        }

        void CreateNewShell(GameObject originalShell)
        {
            originalShell.GetComponent<Renderer>().enabled = false;
            GameObject newShell = GameObject.Instantiate(originalShell);
            newShell.GetComponent<Renderer>().enabled = true;
            newShell.transform.SetParent(originalShell.transform.parent, false);
            newShell.transform.localPosition = originalShell.transform.localPosition;
            newShell.transform.localRotation = originalShell.transform.localRotation;
            newShell.transform.localScale = originalShell.transform.localScale;
            Rigidbody rb = newShell.AddComponent<Rigidbody>();
            rb.velocity = GorillaLocomotion.Player.Instance.bodyCollider.attachedRigidbody.velocity;
            newShell.transform.SetParent(null);
            rb.AddForce((-transform.up + -transform.forward) * -1.5f, ForceMode.Impulse);
            Destroy(newShell, 6f);
        }

        public void UpdateInfo()
        {
            if (Config.ShotgunConfig.isLeft.Value)
            {
                transform.parent.SetParent(GorillaTagger.Instance.offlineVRRig.transform.Find("rig/body/shoulder.L/upper_arm.L/forearm.L/hand.L"), false);
                transform.parent.localPosition = leftPos;
                transform.parent.localRotation = leftRot;
            }
            else
            {
                transform.parent.SetParent(GorillaTagger.Instance.offlineVRRig.transform.Find("rig/body/shoulder.R/upper_arm.R/forearm.R/hand.R"), false);
                transform.parent.localPosition = rightPos;
                transform.parent.localRotation = rightRot;
            }
            audio.volume = Config.ShotgunConfig.volume.Value / 10f;
        }

        public void ChangeForce(bool isLeft)
        {
            switch (isLeft)
            {
                case true:
                    if (Config.ShotgunConfig.force.Value <= 0f) break;
                    Config.ShotgunConfig.force.Value--;
                    break;
                case false:
                    if (Config.ShotgunConfig.force.Value >= 3f) break;
                    Config.ShotgunConfig.force.Value++;
                    break;
            }
        }

        public void ChangeVolume(bool isLeft)
        {
            switch (isLeft)
            {
                case true:
                    if (Config.ShotgunConfig.volume.Value <= 0) break;
                    Config.ShotgunConfig.volume.Value--;
                    UpdateInfo();
                    break;
                case false:
                    if (Config.ShotgunConfig.volume.Value >= 10) break;
                    Config.ShotgunConfig.volume.Value++;
                    UpdateInfo();
                    break;
            }
        }

        public Vector3 rightPos = new Vector3(0.03f, 0f, -0.03f);
        public Quaternion rightRot = Quaternion.Euler(0f, 0f, 165f);
        public Vector3 leftPos = new Vector3(-0.03f, 0f, -0.03f);
        public Quaternion leftRot = Quaternion.Euler(0f, 0f, 195f);
    }
}
