import { Injectable } from '@angular/core';
import * as signalR from '@aspnet/signalr';
import { ChartModel } from '../interfaces/chartdata.model';
import { NbToastrService } from '@nebular/theme';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class SignalRService {
  public bradcastedData: ChartModel[];
  constructor(private toastrService: NbToastrService) {}

  public hubConnection: signalR.HubConnection;

  public startConnection() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(environment.hubUrl)
      .build();

    this.hubConnection
      .start()
      .then((data) => {
        localStorage.setItem('hub', 'established');

        return data;
      })
      .catch((err) => {
        return err;
      });
  }

  public sendClientToServerData(): Promise<any> {
    this.hubConnection.on('sendnotification', (data) => {});
    return Promise.resolve('ready');
  }



  public addBroadcastChartDataListener = () => {
    this.hubConnection.on('broadcastchartdata', (data) => {
      this.bradcastedData = data;
    });
  };

  closeHubConnection() {
    this.hubConnection.stop().then();
  }
}
