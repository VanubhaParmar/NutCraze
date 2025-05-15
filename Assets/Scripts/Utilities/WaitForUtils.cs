using System.Collections;
using UnityEngine;

namespace Tag.NutSort
{
    public static class WaitForUtils
    {
        public static readonly WaitForEndOfFrame EndOfFrame = new WaitForEndOfFrame();
        public static readonly WaitForFixedUpdate FixedUpdate = new WaitForFixedUpdate();

        public static readonly WaitForSeconds OneSecond = new WaitForSeconds(1f);
        public static readonly WaitForSeconds HalfSecond = new WaitForSeconds(0.5f);
        public static readonly WaitForSeconds TenthSecond = new WaitForSeconds(0.1f);
        public static readonly WaitForSeconds ZeroSeconds = new WaitForSeconds(0f);
        public static readonly WaitForSeconds SingleFrame = new WaitForSeconds(0.0001f);

        public static readonly WaitForSecondsRealtime OneSecondRealtime = new WaitForSecondsRealtime(1f);
        public static readonly WaitForSecondsRealtime HalfSecondRealtime = new WaitForSecondsRealtime(0.5f);

        public static IEnumerator Frames(int frameCount)
        {
            if (frameCount <= 0)
                yield break;

            for (int i = 0; i < frameCount; i++)
                yield return EndOfFrame;
        }
       
        public static IEnumerator AudioComplete(this AudioSource audioSource)
        {
            if (audioSource == null) yield break;
            if (!audioSource.isPlaying) yield break;

            while (audioSource.isPlaying)
            {
                yield return null;
            }
        }

        public static IEnumerator ParticleSystemComplete(this ParticleSystem particleSystem)
        {
            if (particleSystem == null) yield break;

            while (particleSystem.isEmitting || particleSystem.particleCount > 0)
            {
                yield return null;
            }
        }
    }
}