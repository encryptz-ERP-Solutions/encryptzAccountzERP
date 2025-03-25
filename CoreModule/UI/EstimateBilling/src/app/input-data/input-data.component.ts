import { Component, OnInit } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CommonService } from '../common.service';
import { map, Observable, startWith } from 'rxjs';

interface Uqc {
  code: string;
  description: string;
}

@Component({
  selector: 'app-input-data',
  templateUrl: './input-data.component.html',
  styleUrls: ['./input-data.component.scss']
})
export class InputDataComponent implements OnInit {
  form!: FormGroup;
  headerData: any;
  taxPercentage: number = 0;
  netValueResult: number = 0;
  gstResult: number = 0;
  totalAmountResult: number = 0;
  igstResult: number = 0;
  cgstResult: number = 0;
  sgstResult: number = 0;
  totalAmtResult: number = 0;
  netTotalValueResult: number = 0;
  totalGstResult: number = 0;
  headerDetailsData: any;
  showError = false;
  uqcDetails: Uqc[] = [];
  filteredUqcDetails: Observable<Uqc[]> = new Observable();

  constructor(private fb: FormBuilder, private service: CommonService) { }

  ngOnInit() {
    this.form = this.fb.group({
      ItemsInfo: this.fb.group({
        description: ['', Validators.required],
        hsnSac: [''],
        qty: ['', [Validators.required, Validators.pattern('^[0-9]*$')]],
        uqc: ['', Validators.required],
        rate: ['', Validators.required],
        taxPercentage: [18, Validators.required],
      }),
      items: this.fb.array([]),
      BankInfo: this.fb.group({
        accountName: [''],
        accountNo: [''],
        ifsCode: ['', [Validators.pattern('[A-Z]{4}0[A-Z0-9]{6}')]],
        bankName: [''],
        declaration: ['We declare that this invoice shows the actual price of the goods described and that all particulars are true and correct'],
      }),
    });

    this.service.eventEmit.subscribe(headerDetails => {
      this.headerDetailsData = headerDetails;
    });
    this.getAllUqc();
    console.log( this.form.get('ItemsInfo'));
  }

  // Getter for the FormArray
  get items(): FormArray {
    return this.form.get('items') as FormArray;
  }

  // Method to add a new row
  addRow() {
    const itemsInfo = this.form.get('ItemsInfo') as FormGroup;
    const itemFormGroup = this.fb.group({
      description: [itemsInfo.get('description')?.value || ''],
      hsnSac: [itemsInfo.get('hsnSac')?.value || ''],
      qty: [itemsInfo.get('qty')?.value || ''],
      uqc: [itemsInfo.get('uqc')?.value || ''],
      rate: [itemsInfo.get('rate')?.value || ''],
      taxPercentage:  [itemsInfo.get('taxPercentage')?.value || '']
    });
   
    this.items.push(itemFormGroup);
    this.resetForm();
  }

   // Method to reset the ItemsInfo form fields
   resetForm() {
    const itemsInfo = this.form.get('ItemsInfo') as FormGroup;
  
    // Create an object with the default values
    let patchValueObject: { [key: string]: any } = {
      description: '',
      qty: '',
      uqc: '',
      rate: ''
    };
    if (this.headerDetailsData?.CompanyInfo?.Gstin) {
      patchValueObject['taxPercentage'] = '';
    }
    itemsInfo.patchValue(patchValueObject);
  }
  

  getAllUqc() {
    this.service.getUqc().subscribe(
      (data: Uqc[]) => {
        this.uqcDetails = data;
        this.filteredUqcDetails = this.form.get('ItemsInfo.uqc')!.valueChanges.pipe(
          startWith(''),
          map(value => this._filterStates(value))
        );
      });
  }

  private _filterStates(value: string): Uqc[] {
    const filterValue = value.toLowerCase();
    return this.uqcDetails.filter(uqc =>
      [uqc.code, uqc.description].some(field => field.toLowerCase().includes(filterValue))
    );
  }

  // Method to update calculations when taxPercentage changes
  updateCalculations(index: number) {
    this.calculateNetValue(index); // Recalculate net value if needed
    this.calculateGST(index); // Recalculate GST
    this.calculateTotalAmount(index); // Recalculate total amount
  }

 

  // Method to remove an item
  removeItem(index: number) {
    this.items.removeAt(index);
  }

  // Method to calculate net value
  calculateNetValue(index: number): number {
    const item = this.items.at(index).value;
    const qty = parseFloat(item.qty) || 0;
    const rate = parseFloat(item.rate) || 0;
    this.netValueResult = qty * rate;
    return parseFloat(this.netValueResult.toFixed(2));
  }

  // Method to calculate GST
  calculateGST(index: number): number {
    // Skip GST calculation if Gstin is null
    if (!this.headerDetailsData?.CompanyInfo?.Gstin) {
      return 0; // Return 0 if Gstin is not available
    }
    const netValue = this.calculateNetValue(index);
    const taxPercentage = parseFloat(this.items.at(index).get('taxPercentage')?.value) || 0;
    this.gstResult = (netValue * taxPercentage) / 100;
    return parseFloat(this.gstResult.toFixed(2));
  }

  // Method to calculate total amount
  calculateTotalAmount(index: number): number {
    const netValue = this.calculateNetValue(index);

    // Skip GST calculation if Gstin is null
    let gst = 0;
    if (this.headerDetailsData?.CompanyInfo?.Gstin != null) {
      gst = this.calculateGST(index);
    }

    this.totalAmountResult = netValue + gst;
    return parseFloat(this.totalAmountResult.toFixed(2));
  }

  // Method to calculate total net value
  getTotalNetValue(): number {
    return this.items.controls.reduce((total, control, index) => {
      this.netTotalValueResult = total + this.calculateNetValue(index);
      return parseFloat(this.netTotalValueResult.toFixed(2));
    }, 0);
  }

  // Method to calculate total GST
  getTotalGST(): number {
    return this.items.controls.reduce((total, control, index) => {
      this.totalGstResult = total + this.calculateGST(index);
      return parseFloat(this.totalGstResult.toFixed(2));
    }, 0);
  }

  // Method to calculate total amount
  getTotalAmount(): number {
    return this.items.controls.reduce((total, control, index) => {
      this.totalAmtResult = total + this.calculateTotalAmount(index);
      return parseFloat(this.totalAmtResult.toFixed(2));
    }, 0);
  }

  // OnSave method
  OnSave() {
    const bankInfoGroup = this.form.get('BankInfo');
    if (bankInfoGroup && !bankInfoGroup.valid) {
      bankInfoGroup.markAllAsTouched();
      return;
    }

    // Extract and prepare bankInfo
    const { BankInfo } = this.form.value;
    let taxableValue = this.netTotalValueResult;
    let totalGst = this.totalGstResult;
    let igst = this.totalGstResult;
    let cgst = this.totalGstResult / 2;
    let sgst = this.totalGstResult / 2;

    // Skip GST and tax if Gstin is null
    if (!this.headerDetailsData?.CompanyInfo?.Gstin) {
      totalGst = 0;
      igst = 0;
      cgst = 0;
      sgst = 0;
    }

    const bankInfo = {
      ...BankInfo,
      taxableValue,
      igst,
      cgst,
      sgst,
      totalAmt: this.getTotalAmount(),
      totalGst,
      Header: this.headerDetailsData
    };

    // Prepare itemsData
    const itemsData = this.items.controls.map((group) => {
      const itemValue = group.value;
      return {
        ...itemValue,
        netValue: this.calculateNetValue(this.items.controls.indexOf(group)),
        gst: this.calculateGST(this.items.controls.indexOf(group)),
        totalAmount: this.calculateTotalAmount(this.items.controls.indexOf(group))
      };
    });

    // Pass data to service
    this.service.passItems(itemsData, bankInfo);
  }
}
