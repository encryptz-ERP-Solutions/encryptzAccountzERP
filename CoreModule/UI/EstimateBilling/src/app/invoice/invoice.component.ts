import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { Subscription } from 'rxjs';
import { CommonService } from '../common.service';

@Component({
  selector: 'app-invoice',
  templateUrl: './invoice.component.html',
  styleUrl: './invoice.component.scss'
})
export class InvoiceComponent implements OnInit {
  headerData: any;
  items: any[] = [];
  bankInfo: any;

  private itemsSubscription!: Subscription;
  constructor(private fb: FormBuilder, private service : CommonService) {   }

  ngOnInit(){

    this.itemsSubscription = this.service.eventEmitItems.subscribe(data => {
      this.items = data.items; 
      this.bankInfo = data.bankInfo;
      console.log('Received items:', this.items, 'Bank Info:', this.bankInfo);
    });
    
  }

  ngOnDestroy() {
    if (this.itemsSubscription) {
      this.itemsSubscription.unsubscribe();
    }
  }
}
