import { CommonModule } from '@angular/common';
import { Component, TemplateRef, ViewChild } from '@angular/core';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatCardModule } from '@angular/material/card';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AdminDataService } from '../../../../core/services/admin-data.service';
import { AdminModule } from '../../../../core/models/admin.models';
import { CommonService } from '../../../../shared/services/common.service';

@Component({
  selector: 'app-modules',
  standalone: true,
  imports: [
    CommonModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatSlideToggleModule,
    MatCardModule,
    ReactiveFormsModule
  ],
  templateUrl: './modules.component.html',
  styleUrl: './modules.component.scss'
})
export class ModulesComponent {
  modules: AdminModule[] = [];
  displayedColumns = ['moduleName', 'system', 'active', 'actions'];
  moduleForm!: FormGroup;
  editingModule: AdminModule | null = null;

  @ViewChild('moduleDialog') moduleDialog!: TemplateRef<any>;

  constructor(
    private adminDataService: AdminDataService,
    private dialog: MatDialog,
    private fb: FormBuilder,
    private commonService: CommonService
  ) {
    this.moduleForm = this.fb.group({
      moduleName: ['', Validators.required],
      isSystemModule: [false],
      isActive: [true]
    });
  }

  ngOnInit(): void {
    this.loadModules();
  }

  loadModules(): void {
    this.adminDataService.getModules().subscribe({
      next: modules => this.modules = modules,
      error: err => this.commonService.showSnackbar(err?.message || 'Unable to load modules', 'ERROR', 3000)
    });
  }

  openDialog(module?: AdminModule): void {
    this.editingModule = module ?? null;
    if (module) {
      this.moduleForm.patchValue(module);
    } else {
      this.moduleForm.reset({
        moduleName: '',
        isSystemModule: false,
        isActive: true
      });
    }
    this.dialog.open(this.moduleDialog, { width: '420px' });
  }

  saveModule(): void {
    if (this.moduleForm.invalid) {
      this.commonService.showSnackbar('Module name is required', 'ERROR', 2500);
      return;
    }
    const payload = this.moduleForm.getRawValue();

    if (this.editingModule) {
      const updated: AdminModule = {
        ...this.editingModule,
        moduleName: payload.moduleName!,
        isSystemModule: payload.isSystemModule ?? false,
        isActive: payload.isActive ?? true
      };
      this.adminDataService.updateModule(updated).subscribe({
        next: () => {
          this.commonService.showSnackbar('Module updated', 'SUCCESS', 2500);
          this.dialog.closeAll();
          this.loadModules();
        },
        error: err => this.commonService.showSnackbar(err?.message || 'Unable to update module', 'ERROR', 3000)
      });
    } else {
      const createPayload = {
        moduleName: payload.moduleName || undefined,
        isSystemModule: payload.isSystemModule ?? false,
        isActive: payload.isActive ?? true
      };
      this.adminDataService.createModule(createPayload).subscribe({
        next: () => {
          this.commonService.showSnackbar('Module created', 'SUCCESS', 2500);
          this.dialog.closeAll();
          this.loadModules();
        },
        error: err => this.commonService.showSnackbar(err?.message || 'Unable to create module', 'ERROR', 3000)
      });
    }
  }

  deleteModule(module: AdminModule): void {
    this.adminDataService.deleteModule(module.moduleID).subscribe({
      next: () => {
        this.commonService.showSnackbar('Module deleted', 'SUCCESS', 2500);
        this.loadModules();
      },
      error: err => this.commonService.showSnackbar(err?.message || 'Unable to delete module', 'ERROR', 3000)
    });
  }
}

