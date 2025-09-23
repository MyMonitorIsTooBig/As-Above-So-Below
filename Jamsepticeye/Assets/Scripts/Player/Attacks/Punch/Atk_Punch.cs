using System.Collections;
using UnityEngine;

public class Atk_Punch : Attack
{

    float distance = 2;

    

    [SerializeField]
    GameObject punchEffect;

    [SerializeField]
    GameObject punchObj;

    [SerializeField] Vector2 _ogWeaponPos;


    protected override void OnEnable()
    {
        base.OnEnable();

        punchObj.transform.localPosition = _ogWeaponPos;
        canMove = true;

        onAttack?.Invoke(true);
    }

    public override void attack()
    {
        //Destroy(EnemyManager.Instance.ClosestEnemy(transform.position, distance));
        //Debug.Log("Attack!");
        if (canMove)
        {
            //transform.position = new Vector3(0.77f, transform.position.y, 0);

            StartCoroutine(punch());

            if(punchObj.TryGetComponent<IAttackObject>(out IAttackObject atkObj))
            {
                atkObj.damage();
            }

        }
    }

    public override void DirectionalAttack(Vector2 direction)
    {
        if (lockAttackAngle)
        {
            if (!canMove)
            {
                return;
            }
        }


        Vector3 targ = (Vector3)direction;

        targ.x = targ.x - punchEffect.transform.localPosition.x;
        targ.y = targ.y - punchEffect.transform.localPosition.y;

        float angle = Mathf.Atan2(targ.y, targ.x) * Mathf.Rad2Deg;

        punchEffect.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, angle));

    }

    IEnumerator punch()
    {
        canMove = false;

        for(int i = 0; i < 10; i++)
        {
            punchObj.transform.localPosition += new Vector3(0.2f,0,0);
            yield return new WaitForSeconds(0.016f);
        }

        for(int i = 0; i < 10; i++)
        {
            punchObj.transform.localPosition -= new Vector3(0.2f, 0, 0);
            yield return new WaitForSeconds(0.016f);
        }

        canMove = true;
        
        onAttack?.Invoke(true);
    }


}
