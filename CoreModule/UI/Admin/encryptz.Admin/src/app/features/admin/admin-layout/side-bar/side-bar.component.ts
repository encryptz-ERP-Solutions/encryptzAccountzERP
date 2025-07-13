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
  menuItems : any = [
    { icon: 'dashboard', label: 'Dashboard', items: [] },
    // {
    //   icon: 'person',
    //   label: 'Profile',
    //   items: [
    //     { icon: 'person', label: 'Personal Info', },
    //     { icon: 'manage_accounts', label: 'Account Settings' },
    //     { icon: 'photo_camera', label: 'Profile Picture' },
    //     { icon: 'work', label: 'Work Information' },
    //   ]
    // },
    { icon: 'account_box', label: 'User Management', items: [] },
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
