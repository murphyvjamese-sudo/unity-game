using UnityEngine;
using UnityEngine.Purchasing;
using System.Collections.Generic;

//left off: read block comment below. I also created a new "Indie" folder in my murphyvjamese gdrive for fresh developer notes, including project designs and general learning notes. (I made one specifically for learning the architecture behind asynchronous and event driven programming. I need to learn these better not only so I can properly / easily implement IAPs and leaderboards, but also for job interviews.
/*
My implementation of this script for IAPs is heavily borrowed from the documentation example at https://docs.unity.com/en-us/iap/upgrade-to-iap-v5#code-sample-of-new-initialization-process-using-storecontroller
*/

public class IAPs: MonoBehaviour
{
    private StoreController storeController;
    private GlobalState gs;

    public async void InitializeIAP()  
    {  
        storeController = UnityIAPServices.StoreController();  
    
        //You might want to uncomment the below line of code if you want to implement a visual indication that your purchase is still processing. It becomes more necessary if you want to prevent duplicate purchases of consumables, but since I only have non-consumables, it should prevent duplicate purchases and be less of an issue.
        //storeController.OnPurchasePending += OnPurchasePending;  
    
        await storeController.Connect();  
    
        storeController.OnProductsFetched += OnProductsFetched;
        storeController.OnPurchasesFetched += OnPurchasesFetched;  
    
        var initialProductsToFetch = new List<ProductDefinition>  
        {  
            new("GameB", ProductType.NonConsumable),  
        };  
    
        storeController.FetchProducts(initialProductsToFetch);  
    }
    void OnProductsFetched(List<Product> products)  
    {  
        // Handle fetched products  
        storeController.FetchPurchases();  
    }  
    void OnPurchasesFetched(Orders orders) {  
    // Process purchases, for example, check for entitlements from completed orders  //me: I think this means I use this to check for Restoring Transactions?
    }
}
