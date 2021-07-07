import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { StorageDriveGridComponent } from './storage-drive-grid.component';

describe('StorageDriveGridComponent', () => {
  let component: StorageDriveGridComponent;
  let fixture: ComponentFixture<StorageDriveGridComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ StorageDriveGridComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(StorageDriveGridComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
