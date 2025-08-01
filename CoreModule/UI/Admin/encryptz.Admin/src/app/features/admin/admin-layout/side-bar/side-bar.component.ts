import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { NavigationEnd, Router } from '@angular/router';
import { filter } from 'rxjs';

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
  menuItems: any = [
    { icon: 'dashboard', label: 'Dashboard', items: [], link: '/admin/dashboard' },
    { icon: 'person', label: 'User Management', items: [], link: '/admin/user-management' }
  ];

  constructor(
    private router: Router) { }

  ngOnInit(): void {
    this.selectedMenu = this.router.url;
    localStorage.setItem('currentAdminMenu', this.selectedMenu);

    // Update selected menu on navigation
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe((event: NavigationEnd) => {
        this.selectedMenu = event.urlAfterRedirects;
        localStorage.setItem('currentAdminMenu', this.selectedMenu);
      });
  }

  openSideNav() {
    this.isOpenSideNav.emit(true);
  }

  closeSideNav() {
    this.isOpenSideNav.emit(false);
  }

  navigateMenu(url: string) {
    this.router.navigateByUrl(url);
    return false;
  }
}
