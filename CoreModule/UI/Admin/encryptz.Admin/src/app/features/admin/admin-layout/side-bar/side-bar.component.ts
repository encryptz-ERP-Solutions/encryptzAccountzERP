import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { NavigationEnd, Router } from '@angular/router';

@Component({
  selector: 'app-side-bar',
  imports: [
    MatIconModule,
    MatButtonModule,
    CommonModule,
    MatDividerModule,
    MatExpansionModule,
    MatTooltipModule
  ],
  templateUrl: './side-bar.component.html',
  styleUrl: './side-bar.component.scss'
})
export class SideBarComponent {

  @Input() isExpand: boolean = false;
  @Output() isOpenSideNav = new EventEmitter<boolean>();
  selectedMenu: any
  menuItems = [
    { icon: 'dashboard', label: 'Dashboard', items: [] },
    {
      icon: 'person',
      label: 'Profile',
      items: [
        { icon: 'person', label: 'Personal Info', },
        { icon: 'manage_accounts', label: 'Account Settings' },
        { icon: 'photo_camera', label: 'Profile Picture' },
        { icon: 'work', label: 'Work Information' },
      ]
    },
    { icon: 'account_box', label: 'Account Overview', items: [] },
    { icon: 'account_balance_wallet', label: 'Income Summary', items: [] },
    { icon: 'money_off', label: 'Expenses Summary', items: [] },
    { icon: 'calculate', label: 'Tax Computation', items: [] },
    { icon: 'receipt_long', label: 'GST Filing', items: [] },
    { icon: 'summarize', label: 'TDS Filing', items: [] },
    { icon: 'fact_check', label: 'Income Tax Filing	', items: [] },
    { icon: 'payments', label: 'Advance Tax', items: [] },
    { icon: 'history', label: 'Tax Payment History	', items: [] },
    { icon: 'article', label: 'Challan Generation', items: [] },
    { icon: 'description', label: 'Form 16 Management', items: [] },
    { icon: 'insert_drive_file', label: 'Form 26AS', items: [] },
    { icon: 'rule', label: 'Audit Reports', items: [] },
    { icon: 'assessment', label: 'Balance Sheet', items: [] },
    { icon: 'bar_chart', label: 'Profit & Loss Statement', items: [] },
    { icon: 'library_books', label: 'Ledger Entries', items: [] },
    { icon: 'account_balance', label: 'Bank Reconciliation', items: [] },
    { icon: 'receipt', label: 'Invoice Management', items: [] },
    { icon: 'note_add', label: 'Credit Notes', items: [] },
    { icon: 'note', label: 'Debit Notes', items: [] },
    { icon: 'group', label: 'Vendor Management', items: [] },
    { icon: 'person_search', label: 'Client Management', items: [] },
    { icon: 'inventory_2', label: 'Asset Register', items: [] },
    { icon: 'functions', label: 'Depreciation Calculator', items: [] },
    { icon: 'event', label: 'Compliance Calendar', items: [] }
  ];

  constructor(
    private router: Router
  ) { }
  ngOnInit() {
    this.selectedMenu = localStorage.getItem('currentAdminMenu');
    this.router.events.subscribe(event => {
      if (event instanceof NavigationEnd) {
        this.selectedMenu = this.router.url.slice(1);
        localStorage.setItem('currentMenu', this.selectedMenu);
      }
    });
  }
  openSideNav() {
    this.isOpenSideNav.emit(true);
  }

  closeSideNav() {
    this.isOpenSideNav.emit(false);
  }

  navigateMenu(url: string) {
    // this.router.navigateByUrl(url);
    this.selectedMenu = url;
    localStorage.setItem('currentAdminMenu', this.selectedMenu)
    return false
  }
}
