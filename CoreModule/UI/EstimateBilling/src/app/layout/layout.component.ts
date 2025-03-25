import { AfterViewInit, ChangeDetectorRef, Component, ViewChild } from '@angular/core';
import { HeaderComponent } from '../header/header.component';
import { MatStepper } from '@angular/material/stepper';
import { InputDataComponent } from '../input-data/input-data.component';

@Component({
  selector: 'app-layout',
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.scss']
})
export class LayoutComponent implements AfterViewInit {
  @ViewChild(HeaderComponent) headerComponent!: HeaderComponent;
  @ViewChild(InputDataComponent) summary!: InputDataComponent;

  public errors: any = {};

  constructor(private cdr: ChangeDetectorRef) { }

  ngAfterViewInit() {
    this.cdr.detectChanges();
  }

  PrintDiv(divId: string) {
    const printContents = document.getElementById(divId)?.innerHTML || '';
    const printWindow = window.open('', '', 'height=600,width=800');
  
    if (printWindow) {
      printWindow.document.open();
      printWindow.document.write(`
        <html>
          <head>
            <title>Tax Invoice</title>
            
            <link rel="stylesheet" href="assets/css/print.css" />
            <style>
              table {
                width: 100%;
                border-collapse: collapse;
              }
              table, th, td {
                border: 1px solid black;
              }
              th, td {
                padding: 5px;
                text-align: left;
              }
              th {
                background-color: #f2f2f2; /* Light grey background for header */
                color: #333; /* Dark grey text color */
                font-weight: bold;
              }
              .hide-when-print {
                display: none !important;
              }
              h2 {
                margin-bottom: 10px;
              }
            </style>
          </head>
          <body>
            <div class="container mt-3">
              ${printContents}
            </div>
            <script>
              window.onload = function() {
                window.print();
                window.onafterprint = function() {
                  window.close();
                }
              }
            </script>
          </body>
        </html>
      `);
      printWindow.document.close();
    }
  }

  ResetAll() {
    this.headerComponent.headerForm.reset();
    this.summary.form.reset();
    this.summary.igstResult = 0;
    this.summary.sgstResult = 0;
    this.summary.cgstResult = 0;
  }

  GoForward(stepper: MatStepper) {
    const currentStepIndex = stepper.selectedIndex;

    if (currentStepIndex === 0) {
      if (this.headerComponent.headerForm.valid) {
        this.headerComponent.OnSave();
        stepper.next();
      } else {
        this.headerComponent.headerForm.markAllAsTouched();
      }
    } else if (currentStepIndex === 1) {
      if (this.summary.form.get('BankInfo')?.valid) {
        this.summary.OnSave();
        stepper.next();
      } else {
        this.summary.form.get('BankInfo')?.markAllAsTouched();
      }
    }
  }

  GoBack(stepper: MatStepper) {
    stepper.previous();
  }
}
