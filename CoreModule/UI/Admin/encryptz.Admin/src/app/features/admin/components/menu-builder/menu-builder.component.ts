import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Component, TemplateRef, ViewChild } from '@angular/core';
import { MatTreeModule } from '@angular/material/tree';
import { NestedTreeControl } from '@angular/cdk/tree';
import { MatTreeNestedDataSource } from '@angular/material/tree';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AdminDataService } from '../../../../core/services/admin-data.service';
import { AdminMenuItem, AdminModule, MenuTreeNode } from '../../../../core/models/admin.models';
import { CommonService } from '../../../../shared/services/common.service';
import { ConfirmationComponent } from '../../../../shared/components/confirmation/confirmation.component';
import { finalize } from 'rxjs';

@Component({
  selector: 'app-menu-builder',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatTreeModule,
    MatIconModule,
    MatButtonModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatSlideToggleModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatTooltipModule,
    ReactiveFormsModule
  ],
  templateUrl: './menu-builder.component.html',
  styleUrl: './menu-builder.component.scss'
})
export class MenuBuilderComponent {
  treeControl = new NestedTreeControl<MenuTreeNode>(node => node.children);
  dataSource = new MatTreeNestedDataSource<MenuTreeNode>();
  modules: AdminModule[] = [];
  selectedModuleId: number | null = null;
  flatMenu: AdminMenuItem[] = [];
  editingMenu: AdminMenuItem | null = null;
  parentId: number | undefined = undefined;
  loading = false;
  loadingModules = false;

  menuForm!: FormGroup;

  @ViewChild('menuDialog') menuDialog!: TemplateRef<any>;

  constructor(
    private adminDataService: AdminDataService,
    private dialog: MatDialog,
    private fb: FormBuilder,
    private commonService: CommonService
  ) {
    this.menuForm = this.fb.group({
      menuText: ['', Validators.required],
      menuURL: [''],
      iconClass: ['menu'],
      displayOrder: [1, Validators.required],
      isActive: [true]
    });
  }

  ngOnInit(): void {
    this.loadModules();
  }

  hasChild = (_: number, node: MenuTreeNode) => !!node.children && node.children.length > 0;

  loadModules(): void {
    this.loadingModules = true;
    this.adminDataService.getModules()
      .pipe(finalize(() => this.loadingModules = false))
      .subscribe({
        next: (modules: AdminModule[]) => {
          console.log('Modules loaded:', modules);
          this.modules = modules.filter(m => m.isActive);
          console.log('Active modules:', this.modules);
          // Auto-select Admin Control Center if available, otherwise select first module
          const adminModule = modules.find(m => 
            m.moduleName?.toLowerCase().includes('admin') ||
            m.moduleName?.toLowerCase() === 'admin control center'
          );
          if (adminModule) {
            console.log('Selected admin module:', adminModule);
            this.selectedModuleId = adminModule.moduleID;
            this.loadMenuTree();
          } else if (modules.length > 0) {
            console.log('Selected first module:', modules[0]);
            this.selectedModuleId = modules[0].moduleID;
            this.loadMenuTree();
          } else {
            console.warn('No modules found');
          }
        },
        error: err => {
          console.error('Error loading modules:', err);
          this.commonService.showSnackbar(err?.message || 'Unable to load modules', 'ERROR', 3000);
        }
      });
  }

  onModuleChange(): void {
    if (this.selectedModuleId) {
      this.loadMenuTree();
    } else {
      this.dataSource.data = [];
      this.flatMenu = [];
    }
  }

  loadMenuTree(): void {
    if (!this.selectedModuleId) {
        return;
      }

    this.loading = true;
    this.commonService.loaderState(true);
    this.adminDataService.getMenuItemsByModule(this.selectedModuleId)
      .pipe(finalize(() => {
        this.loading = false;
        this.commonService.loaderState(false);
      }))
      .subscribe({
        next: items => {
          console.log('Menu items loaded:', items);
          console.log('Selected module ID:', this.selectedModuleId);
          this.flatMenu = items || [];
          
          if (items && items.length > 0) {
            const tree = this.adminDataService.buildMenuTree(items);
            console.log('Menu tree built:', tree);
            console.log('Tree length:', tree.length);
            
            // Properly set the data source
            this.dataSource.data = tree;
            this.treeControl.dataNodes = tree;
            
            // Expand all nodes
        this.treeControl.expandAll();
            
            console.log('DataSource data after assignment:', this.dataSource.data);
            console.log('DataSource data length:', this.dataSource.data?.length);
          } else {
            console.log('No menu items found for module');
            this.dataSource.data = [];
            this.flatMenu = [];
          }
        },
        error: err => {
          console.error('Error loading menu items:', err);
          this.commonService.showSnackbar(err?.message || 'Unable to load menu items', 'ERROR', 3000);
          this.dataSource.data = [];
          this.flatMenu = [];
        }
    });
  }

  openDialog(parentId?: number | null, menu?: AdminMenuItem): void {
    this.parentId = parentId ?? undefined;
    this.editingMenu = menu ?? null;

    if (menu) {
      this.menuForm.patchValue({
        menuText: menu.menuText,
        menuURL: menu.menuURL,
        iconClass: menu.iconClass || 'menu',
        displayOrder: menu.displayOrder,
        isActive: menu.isActive
      });
    } else {
      this.menuForm.reset({
        menuText: '',
        menuURL: '',
        iconClass: 'menu',
        displayOrder: 1,
        isActive: true
      });
    }

    this.dialog.open(this.menuDialog, { width: '520px' });
  }

  saveMenu(): void {
    if (!this.selectedModuleId) {
      this.commonService.showSnackbar('Please select a module', 'ERROR', 3000);
      return;
    }
    if (this.menuForm.invalid) {
      this.commonService.showSnackbar('Please complete required fields', 'ERROR', 2500);
      return;
    }
    const payload = this.menuForm.getRawValue();

    if (this.editingMenu) {
      const updated: AdminMenuItem = {
        ...this.editingMenu,
        menuText: payload.menuText!,
        menuURL: payload.menuURL || undefined,
        iconClass: payload.iconClass || 'menu',
        displayOrder: payload.displayOrder ?? 1,
        isActive: payload.isActive ?? true,
        moduleID: this.selectedModuleId!,
        parentMenuItemID: this.editingMenu.parentMenuItemID
      };
      this.adminDataService.updateMenuItem(updated).subscribe({
        next: () => {
          this.commonService.showSnackbar('Menu item updated', 'SUCCESS', 2500);
          this.dialog.closeAll();
          this.loadMenuTree();
        },
        error: err => this.commonService.showSnackbar(err?.message || 'Unable to update menu item', 'ERROR', 3000)
      });
    } else {
      const newItem: Partial<AdminMenuItem> = {
        moduleID: this.selectedModuleId!,
        parentMenuItemID: this.parentId,
        menuText: payload.menuText!,
        menuURL: payload.menuURL || undefined,
        iconClass: payload.iconClass || 'menu',
        displayOrder: payload.displayOrder ?? 1,
        isActive: payload.isActive ?? true
      };
      this.adminDataService.createMenuItem(newItem).subscribe({
        next: () => {
          this.commonService.showSnackbar('Menu item created', 'SUCCESS', 2500);
          this.dialog.closeAll();
          this.loadMenuTree();
        },
        error: err => this.commonService.showSnackbar(err?.message || 'Unable to create menu item', 'ERROR', 3000)
      });
    }
  }

  deleteMenu(menu: AdminMenuItem): void {
    const dialogRef = this.dialog.open(ConfirmationComponent, {
      width: '400px',
      data: {
        title: 'Delete menu item',
        description: `Are you sure you want to delete "${menu.menuText}"? This will also delete all child menu items. This action cannot be undone.`
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
    this.adminDataService.deleteMenuItem(menu.menuItemID).subscribe({
      next: () => {
        this.commonService.showSnackbar('Menu item deleted', 'SUCCESS', 2500);
        this.loadMenuTree();
      },
      error: err => this.commonService.showSnackbar(err?.message || 'Unable to delete menu item', 'ERROR', 3000)
    });
      }
    });
  }

  get selectedModuleName(): string {
    const module = this.modules.find(m => m.moduleID === this.selectedModuleId);
    return module?.moduleName || 'Select a module';
  }
}

