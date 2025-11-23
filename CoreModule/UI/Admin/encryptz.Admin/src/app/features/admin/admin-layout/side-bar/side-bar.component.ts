import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDividerModule } from '@angular/material/divider';
import { MatExpansionModule } from '@angular/material/expansion';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { NavigationEnd, Router } from '@angular/router';
import { filter, finalize, of, switchMap } from 'rxjs';
import { AdminDataService } from '../../../../core/services/admin-data.service';
import { CommonService } from '../../../../shared/services/common.service';
import { MenuTreeNode } from '../../../../core/models/admin.models';

interface AdminNavItem {
  icon?: string;
  label: string;
  link?: string;
  children: AdminNavItem[];
}

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
  selectedMenu: string | null = null;
  menuItems: AdminNavItem[] = [];
  loadingMenu = false;

  constructor(
    private router: Router,
    private adminDataService: AdminDataService,
    private commonService: CommonService) { }

  ngOnInit(): void {
    this.selectedMenu = this.router.url;
    localStorage.setItem('currentAdminMenu', this.selectedMenu);
    this.loadMenu();

    // Update selected menu on navigation
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe((event: NavigationEnd) => {
        this.selectedMenu = event.urlAfterRedirects;
        localStorage.setItem('currentAdminMenu', this.selectedMenu);
      });
  }

  private loadMenu(): void {
    this.loadingMenu = true;
    this.commonService.loaderState(true);

    this.adminDataService.getModules()
      .pipe(
        switchMap(modules => {
          const adminModule = modules.find(m => 
            m.moduleName?.toLowerCase() === 'admin control center' ||
            m.moduleName?.toLowerCase().includes('admin')
          );
          if (!adminModule) {
            return of<AdminNavItem[]>(this.getFallbackMenu());
          }
          return this.adminDataService.getMenuItemsByModule(adminModule.moduleID)
            .pipe(
              switchMap(menuItems => {
                const tree = this.adminDataService.buildMenuTree(menuItems);
                return of(this.mapTreeToNav(tree));
              })
            );
        }),
        finalize(() => {
          this.loadingMenu = false;
          this.commonService.loaderState(false);
        })
      )
      .subscribe({
        next: items => {
          this.menuItems = (items && items.length > 0)
            ? items
            : this.getFallbackMenu();
        },
        error: () => {
          this.menuItems = this.getFallbackMenu();
        }
      });
  }

  private mapTreeToNav(nodes: MenuTreeNode[]): AdminNavItem[] {
    return nodes.map(node => ({
      icon: node.iconClass || 'menu',
      label: node.menuText,
      link: node.menuURL || undefined,
      children: this.mapTreeToNav(node.children)
    }));
  }

  private getFallbackMenu(): AdminNavItem[] {
    return [
      { icon: 'dashboard', label: 'Dashboard', link: '/admin/dashboard', children: [] },
      { icon: 'group', label: 'User Management', link: '/admin/user-management', children: [] },
      { icon: 'business', label: 'Business Management', link: '/admin/business-management', children: [] },
      {
        icon: 'admin_panel_settings',
        label: 'Security Center',
        children: [
          { icon: 'badge', label: 'Roles', link: '/admin/roles', children: [] },
          { icon: 'checklist', label: 'Permissions', link: '/admin/permissions', children: [] }
        ]
      },
      {
        icon: 'settings',
        label: 'System Setup',
        children: [
          { icon: 'widgets', label: 'Modules', link: '/admin/modules', children: [] },
          { icon: 'menu', label: 'Menu Builder', link: '/admin/menu-builder', children: [] }
        ]
      },
      { icon: 'price_change', label: 'Subscription Plans', link: '/admin/subscription-plans', children: [] },
      { icon: 'history', label: 'Audit & Activity', link: '/admin/audit-log', children: [] }
    ];
  }

  openSideNav() {
    this.isOpenSideNav.emit(true);
  }

  closeSideNav() {
    this.isOpenSideNav.emit(false);
  }

  navigateMenu(url?: string) {
    if (!url) {
      return false;
    }
    this.router.navigateByUrl(url);
    return false;
  }
}
