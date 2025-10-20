import { Component, computed, Signal } from '@angular/core';
import { CommonService } from '../../services/common.service';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-loader',
  imports: [
    CommonModule
  ],
  templateUrl: './loader.component.html',
  styleUrl: './loader.component.scss'
})
export class LoaderComponent {

  isPreLoader: Signal<boolean>;

  constructor(
    private commonService: CommonService,
    // public themeService: ThemeService
  ) {
    this.isPreLoader = computed(() => this.commonService.isLoader());
  }


}
