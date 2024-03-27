using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class RaidusDamageGun : Gun
{
    [SerializeField] Camera cam;
    private float explosionRadius = 5f; 
    private float explosionDelay = 1f;

    //[SerializeField] GameObject ExplosionVolume;
    Vector3 targetPosition;

    GameObject _explosionVolume;

    public override void Use()
    {
        RadiusDamage();
    }

    public void RadiusDamage()
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        ray.origin = cam.transform.position;
        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            Debug.Log("Launcher hit on: " + hitInfo.collider.gameObject.name);
            targetPosition = hitInfo.point;

            GameObject explosionVolume = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "LauncherArea") , targetPosition, Quaternion.identity);

            _explosionVolume = explosionVolume;

            //PhotonNetwork.Destroy(explosionVolume);


            // Invoke the explosion after the delay
            Invoke("Explode", explosionDelay);




        }
    }

    

    private void Explode()
    {
        PhotonNetwork.Destroy(_explosionVolume);
        // Find all colliders in the explosion radius
        Collider[] colliders = Physics.OverlapSphere(targetPosition, explosionRadius);

        // Loop through each collider
        foreach (Collider col in colliders)
        {
            col.gameObject.GetComponent<IDamageable>()?.TakeDamage(((GunInfo)itemInfo).damage);
        }        

    }
}
