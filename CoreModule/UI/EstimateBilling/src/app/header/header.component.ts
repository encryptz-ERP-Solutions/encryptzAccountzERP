import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { FormArray, FormBuilder, FormControl, FormGroup, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { CommonService } from '../common.service';
import { map, Observable, startWith } from 'rxjs';
import { DatePipe } from '@angular/common';

interface State {
  STATE_NAME: string;
  GST_CODE: string;
}


@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
})
export class HeaderComponent implements OnInit {
  headerForm!: FormGroup;
  imagePreview: any;
  states: State[] = [];
  filteredStatesCompanyInfo: Observable<State[]> = new Observable();
  filteredStatesBillingDetails: Observable<State[]> = new Observable();
  filteredStatesConsigneeStateDetails: Observable<State[]> = new Observable();

  @ViewChild('fileInput') fileInput!: ElementRef;

  constructor(private fb: FormBuilder, private service: CommonService, private toastr: ToastrService, private datePipe: DatePipe) { }

  ngOnInit() {
    const today = new Date();
    this.headerForm = this.fb.group({      
      InvoiceDetails: this.fb.group({
        InvoiceNumber: ['001', Validators.required],
        InvoiceDate: [today , Validators.required],
        OrderNo: [''],
        OrderDate: [''],
        EwayBillNo: [''],
        EwayDate: [''],
      }),
      BillingDetails: this.fb.group({
        BillCompanyAddress : [''],
        BillCompName: ['', Validators.required],
        BillPhoneNumber: ['' , [ Validators.pattern(/^\d{10}$/)]],
        BillingState: ['', Validators.required],
        BillingCityTown: [''],
        BillingPostalCode: ['', [ Validators.pattern('^[0-9]*$')]],
        BillingPlace: [''],
        BillingGSTIN: ['', [Validators.pattern(/^[A-Z0-9]{15}$/)]], // Regex validation for GSTIN        
      }),
      ConsigneeDetails: this.fb.group({
        ConsigneeName: [''],
        ConsigneeAddress: [''],
        ConsigneePhone: ['' , [ Validators.pattern(/^\d{10}$/)]],
        ConsigneeState: [''],
        ConsigneeGSTIN: ['', [Validators.pattern(/^[A-Z0-9]{15}$/)]], // Regex validation for GSTIN
        ConsigneeCityTown: [''],
        ConsigneePostalCode: ['', [ Validators.pattern('^[0-9]*$')]],
        ConsigneePlace: [''],
      }),
      CompanyInfo: this.fb.group({
        CompanyPlace: [''],
        CompanyCityTown: [''],
        CompanyPostalCode: ['', [ Validators.pattern('^[0-9]*$')]],
        CompanyName: ['', Validators.required],
        CompanyAddress: ['', Validators.required],
        PhoneNumber: ['' , [ Validators.pattern(/^\d{10}$/)]],
        WebSite: [''],
        Email: ['', [Validators.email]],
        State: ['', Validators.required],
        Gstin: ['', [Validators.pattern(/^[A-Z0-9]{15}$/)]],
      }),
      Fileditems: this.fb.array([]) // Start with an empty array
    });

    this.getAllStates();

    this.headerForm.get('BillingDetails')?.valueChanges.subscribe(billingDetails => {
      this.updateConsigneeDetails(billingDetails);
    });

    // Set up listeners for GSTIN changes to update state
    this.setupGstinChangeListener('CompanyInfo.Gstin', 'CompanyInfo.State');
    this.setupGstinChangeListener('BillingDetails.BillingGSTIN', 'BillingDetails.BillingState');
    this.setupGstinChangeListener('ConsigneeDetails.ConsigneeGSTIN', 'ConsigneeDetails.ConsigneeState');
  }

  updateConsigneeDetails(billingDetails: any): void {
    const consigneeDetails = {
      ConsigneeName: billingDetails.BillCompName,
      ConsigneePhone: billingDetails.BillPhoneNumber,
      ConsigneeAddress: billingDetails.BillCompanyAddress,
      ConsigneeCityTown: billingDetails.BillingCityTown,
      ConsigneePostalCode: billingDetails.BillingPostalCode,
      ConsigneePlace: billingDetails.BillingPlace,
      ConsigneeGSTIN: billingDetails.BillingGSTIN,
      ConsigneeState: billingDetails.BillingState
    };
    
    // Update ConsigneeDetails form group with new values
    this.headerForm.get('ConsigneeDetails')?.patchValue(consigneeDetails);
  }

  get Fileditems(): FormArray {
    return <FormArray>this.headerForm.get('Fileditems');
  }

  createItem(): FormGroup {
    return this.fb.group({
      FieldName: [''],
      FieldValue: ['']
    });
  }

  addRow(): void {
    if (this.Fileditems.length < 4) {
      this.Fileditems.push(this.createItem());
    } else {
      console.log('Maximum number of rows (3) reached');
    }
  }
  removeRow(index: number): void {
    const control = <FormArray>this.headerForm.get('Fileditems');
    if (control.length > 0) {
      control.removeAt(index);
    }
  }
  

  private setupGstinChangeListener(gstinControlPath: string, stateControlPath: string) {
    this.headerForm.get(gstinControlPath)?.valueChanges.subscribe(gstin => {
      const state = this.getStateByGstin(gstin);
      this.headerForm.get(stateControlPath)?.setValue(state || '', { emitEvent: false });
    });
  }

  getAllStates() {
    this.service.getStates().subscribe(
      (data: State[]) => {
        this.states = data;

        // Filtering for CompanyInfo.State
        this.filteredStatesCompanyInfo = this.headerForm.get('CompanyInfo.State')!.valueChanges.pipe(
          startWith(''),
          map(value => this._filterStates(value))
        );

        // Filtering for BillingDetails.BillingState
        this.filteredStatesBillingDetails = this.headerForm.get('BillingDetails.BillingState')!.valueChanges.pipe(
          startWith(''),
          map(value => this._filterStates(value))
        );

        // Filtering for ConsigneeDetails.ConsigneeState
        this.filteredStatesConsigneeStateDetails = this.headerForm.get('ConsigneeDetails.ConsigneeState')!.valueChanges.pipe(
          startWith(''),
          map(value => this._filterStates(value))
        );
      },
      error => {
        console.error('Error fetching states:', error);
      }
    );
  }

  getStateByGstin(gstin: string): string | null {
    const stateCode = gstin.substring(0, 2); // Extract state code from GSTIN
    const state = this.states.find(state => state.GST_CODE === stateCode);
    return state ? state.STATE_NAME : null;
  }

  private _filterStates(value: string): State[] {
    const filterValue = value.toLowerCase();
    return this.states.filter(state => state.STATE_NAME.toLowerCase().includes(filterValue));
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      const file = input.files[0];
      const reader = new FileReader();

      reader.onload = () => {
        this.imagePreview = reader.result;
      };

      reader.readAsDataURL(file);
    }
  }

  public OnSave() {
    if (!this.headerForm.valid) {
      this.headerForm.markAllAsTouched();
      return;
    }

    const formData = this.headerForm.value;
    const combinedData = {
      ...formData,
      image: this.imagePreview
    };

    this.service.PassToAnotherComp(combinedData);
  }
}
