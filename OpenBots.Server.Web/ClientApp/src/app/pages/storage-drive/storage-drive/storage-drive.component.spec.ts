import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { StorageDriveComponent } from './storage-drive.component';

describe('StorageDriveComponent', () => {
  let component: StorageDriveComponent;
  let fixture: ComponentFixture<StorageDriveComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ StorageDriveComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(StorageDriveComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
