using UnityEngine;

public static class Effects
{
    public static void BloodExplosion(Vector3 position, int amount) 
    {
        GameObject bloodParticleObject = GameObject.Find("Blood Particles");

        GameObject particleObject = GameObject.Instantiate(bloodParticleObject);
        ParticleSystem particleSystem = particleObject.GetComponent<ParticleSystem>();

        GameObject.Destroy(particleObject, 5);

        particleObject.transform.position = position;
        particleSystem.Emit(150);
    }
}
