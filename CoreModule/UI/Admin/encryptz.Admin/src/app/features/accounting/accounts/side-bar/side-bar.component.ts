import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-side-bar',
  imports: [
    MatIconModule,
    MatButtonModule,
    CommonModule,
    MatDividerModule
  ],
  templateUrl: './side-bar.component.html',
  styleUrl: './side-bar.component.scss',
  standalone: true
})
export class SideBarComponent {

  @Input() isExpand: boolean = false;
  @Output() isOpenSideNav = new EventEmitter<boolean>();
  menuItems = [
    {
      icon: 'person',
      label: 'Person',
      items: [
        { icon: 'person', label: 'Sub Menu Person', },
        { icon: 'paid', label: 'Sub Menu Paid' },
        { icon: 'corporate_fare', label: 'Corporate' },
        { icon: 'euro', label: 'Euro' },
      ]
    },
    { icon: 'paid', label: 'Paid' },
    { icon: 'corporate_fare', label: 'Corporate' },
    { icon: 'euro', label: 'Euro' },
    { icon: 'person', label: 'Person' },
    { icon: 'paid', label: 'Paid' },
    { icon: 'corporate_fare', label: 'Corporate' },
    { icon: 'euro', label: 'Euro' },
    { icon: 'person', label: 'Person' },
    { icon: 'paid', label: 'Paid' },
    {
      icon: 'corporate_fare', label: 'Corporate',
      items: [
        { icon: 'person', label: 'Person', },
        { icon: 'paid', label: 'Paid' }
      ]
    },
    { icon: 'euro', label: 'Euro' },
    { icon: 'person', label: 'Person' },
    { icon: 'paid', label: 'Paid' },
    { icon: 'corporate_fare', label: 'Corporate' },
    { icon: 'euro', label: 'Euro' },
    { icon: 'person', label: 'Person' },
    { icon: 'paid', label: 'Paid' },
    { icon: 'corporate_fare', label: 'Corporate' },
    { icon: 'euro', label: 'Euro' },
    { icon: 'paid', label: 'Paid' },
    { icon: 'corporate_fare', label: 'Corporate' },
    { icon: 'euro', label: 'Euro' },
    { icon: 'person', label: 'Person' },
    { icon: 'paid', label: 'Paid' },
    { icon: 'corporate_fare', label: 'Corporate' },
    { icon: 'euro', label: 'Euro' },
    { icon: 'person', label: 'Person' },
    { icon: 'paid', label: 'Paid' },
    { icon: 'corporate_fare', label: 'Corporate' },
    { icon: 'euro', label: 'Euro' }

  ];

  openSideNav() {
    this.isOpenSideNav.emit(true);
  }
}
