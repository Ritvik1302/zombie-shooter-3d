using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopDetector : MonoBehaviour {
	public Transform shootPoint;
	public float detectRange;
	public Text shopText;
	public Text warningText;
	public AudioClip purchasedSound;

	IEnumerator warningTextCo = null;

	IEnumerator HideWarningText() {
		yield return new WaitForSeconds(3f);

		warningText.text = "";
		yield break;
	}
	
	void Start() {
		shopText = GameObject.Find("UI/InGameUI/Info/ShopText").GetComponent<Text>();
		warningText = GameObject.Find("UI/InGameUI/Info/WarningText").GetComponent<Text>();
	}

	void PrintWarning(string text) {
		if(warningTextCo != null) StopCoroutine(warningTextCo);

		warningTextCo = HideWarningText();
		warningText.text = text;

		StartCoroutine(warningTextCo);
	}

	void BuyWeapon(Weapon weapon) {
		string weaponName = weapon.ToString();
		WeaponManager weaponManager = transform.Find("WeaponHolder").GetComponent<WeaponManager>();
		GameObject weaponGO = transform.Find("WeaponHolder/" + weaponName).gameObject;
		weaponManager.currentWeaponGO.GetComponent<WeaponBase>().Unload();

		weaponManager.currentWeapon = weapon;
		weaponManager.currentWeaponGO = weaponGO;

		weaponManager.primaryWeapon = weapon;
		weaponManager.primaryWeaponGO = weaponGO;

		WeaponBase weaponBase = weaponManager.currentWeaponGO.GetComponent<WeaponBase>();

		weaponManager.currentWeaponGO.SetActive(true);
		weaponBase.InitAmmo();
		weaponBase.Draw();
	}

	

	int GetAmmoPrice(Weapon weapon) {
		int price = 0;

		switch(weapon) {
			case Weapon.AKM:
				price = 250;
				break;
			case Weapon.M870:
				price = 200;
				break;
			case Weapon.MP5K:
				price = 150;
				break;
			case Weapon.Glock:
				price = 90;
				break;
			default:
				price = 100;
				break;
		}

		return price;
	}

	

	void Update() {
		RaycastHit hit;
		Vector3 position = shootPoint.position;
		position.y += 1;

	
		if(Physics.Raycast(position, transform.TransformDirection(Vector3.forward * detectRange), out hit, detectRange)) {
			if(hit.transform.tag == "Shop") {
				Shop shop = hit.transform.GetComponent<Shop>();
				ShopType shopType = shop.shopType;
				string shopTitle = shop.title;
				string shopDesc = shop.description;
				int shopPrice = shop.price;
				bool isPurchasable = true;

				WeaponManager weaponManager = transform.Find("WeaponHolder").GetComponent<WeaponManager>();
				WeaponBase weaponBase = weaponManager.currentWeaponGO.GetComponent<WeaponBase>();
				Weapon weapon = weaponManager.currentWeapon;

				if(shopType == ShopType.AMMO) {
					shopPrice = GetAmmoPrice(weapon);
					shopText.text = shopTitle + "\n(" + shopPrice + "$)\n\n" +  shopDesc + "\n\n";
				}
				
				
				else {
					shopText.text = shopTitle + "\n(" + shopPrice + "$)\n\n" +  shopDesc + "\n\n";
				}

				if(isPurchasable && Input.GetKeyDown(KeyCode.F)) {
					FundSystem fundSystem = transform.parent.GetComponent<FundSystem>();
					int fund = fundSystem.GetFund();

					if(fund < shopPrice) {
						PrintWarning("Not enough money!");
					}
					else {
						bool wasPurchased = true;

						if(shopType == ShopType.AMMO) {
							weaponBase.bulletsLeft = weaponBase.startBullets + weaponBase.bulletsPerMag;
							weaponBase.UpdateAmmoText();
						}
						else if(shopType == ShopType.WEAPON_MP5K) {
							if(!weaponManager.HasWeapon(Weapon.MP5K)) {
								BuyWeapon(Weapon.MP5K);
							}
							else {
								wasPurchased = false;
								PrintWarning("You already have weapon.");
							}
						}
						else if(shopType == ShopType.WEAPON_AKM) {
							if(!weaponManager.HasWeapon(Weapon.AKM)) {
								BuyWeapon(Weapon.AKM);
							}
							else {
								wasPurchased = false;
								PrintWarning("You already have weapon.");
							}
						}
						else if(shopType == ShopType.WEAPON_M870) {
							if(!weaponManager.HasWeapon(Weapon.M870)) {
								BuyWeapon(Weapon.M870);
							}
							else {
								wasPurchased = false;
								PrintWarning("You already have weapon.");
							}
						
						
						}
						else {
							wasPurchased = false;
						}

						if(wasPurchased) {
							fundSystem.TakeFund(shopPrice);
							SoundManager soundManager = transform.Find("SoundManager").GetComponent<SoundManager>();
							soundManager.Play(purchasedSound);
						}
					}
				}
			}
		}
		else {
			shopText.text = "";
		}
	}
}
