import React from 'react';

import Page from '../../components/Page';
import Card from '../../components/cards/Card';

import styles from "../../styles/pages/legal/Legal.module.css";


const headerSection = (pageStyles) => { return(
  <header className={pageStyles.header}>
    <div className={styles.header}>
      <h3>Privacy Policy for Janus Version Control System</h3>
      <p><strong>Last Updated:</strong> 18/03/2025</p>
    </div>
  </header>
)};


const PrivacyPolicy = () => (
  <Page header={headerSection}>

    <Card>
      <div className={styles.holder}>
        <section>
          <h4>1. Introduction</h4>
          <p>
            This Privacy Policy explains how Janus (“we”, “us”, or “our”) collects, uses, stores, and protects information for the Janus Version Control System (“Janus”). Janus is hosted on-premise by your organization, and access is restricted to authorized users. By using Janus, you agree to the practices described in this Privacy Policy. If you do not agree with this policy, you should not use the Service.
          </p>
        </section>

        <section>
          <h4>2. Information We Collect</h4>
          <ul>
            <li><strong>Account Information:</strong>When you are provided with or create an account, we collect personal information such as your name, email address, username, and other details required by your organization.</li>
            <li><strong>Usage Data:</strong> We may collect data regarding your usage of Janus, including login times, accessed repositories, and system interactions.</li>
            <li><strong>Technical Data:</strong> Information about the device, browser, or other tools used to access Janus may be collected to assist in troubleshooting and system improvements.</li>
          </ul>
        </section>
        

        <section>
          <h4>3. How We Use Your Information</h4>
          <ul>
            <li><strong>Provision of Service:</strong> The information collected is used to manage and operate Janus, authenticate users, and ensure the proper functioning of the Service.</li>
            <li><strong>Security and Maintenance:</strong> Usage and technical data help us monitor system performance, enhance security measures, and troubleshoot issues.</li>
            <li><strong>Compliance:</strong> We use the collected information to comply with legal obligations, internal policies, and auditing requirements as mandated by your organization.</li>
          </ul>
        </section>

        <section>
          <h4>4. Data Storage and Security</h4>
          <ul>
            <li><strong>On-Premise Hosting:</strong> Janus is hosted on-premise, and all data is stored on servers maintained by your organization.</li>
            <li><strong>Security Measures:</strong> We implement appropriate physical, technical, and administrative measures to protect your information against unauthorized access, alteration, or loss.</li>
            <li><strong>Access Control:</strong> Access to data is strictly limited to authorized personnel who require it to perform their job functions, and all access is subject to your organization’s security policies.</li>
          </ul>
        </section>

        <section>
          <h4>5. Data Sharing and Disclosure</h4>
          <ul>
            <li><strong>Internal Use:</strong> Your personal and usage data may be shared with relevant departments within your organization for support, auditing, or system improvement purposes.</li>
            <li><strong>Third Parties:</strong> Janus does not share your information with third parties outside your organization, except as required by law or with express authorization from your organization.</li>
            <li><strong>Legal Compliance:</strong> We may disclose information if required to do so by law, regulation, or legal process.</li>
          </ul>
        </section>

        <section>
          <h4>6. Data Retention</h4>
          <p>
          Your information is retained for as long as necessary to fulfil the purposes for which it was collected or as required by your organization’s policies and applicable law. Once the information is no longer required, it will be securely deleted or anonymized.
          </p>
        </section>

        <section>
          <h4>7. User Rights and Responsibilities</h4>
          <ul>
            <li><strong>Accuracy:</strong> It is your responsibility to ensure that your account information is accurate and up-to-date.</li>
            <li><strong>Access and Correction:</strong> Subject to your organization’s policies, you may have the right to access and request correction of your data.</li>
            <li><strong>Security Practices:</strong> You are encouraged to follow best practices for password security and notify your IT department if you suspect any unauthorized access or data breaches.</li>
          </ul>
        </section>

        <section>
          <h4>8. Monitoring and Updates</h4>
          <p>
            We periodically review our data collection and processing practices to ensure compliance with applicable laws and the needs of your organization. This Privacy Policy may be updated from time to time. You will be notified of any material changes as required by your organization’s internal policies.
          </p>
        </section>

        <section>
          <h4>9. Contact Information</h4>
          <p>
            If you have any questions about this Privacy Policy or our data handling practices, please contact Janus at:
          </p>
          <p>
            Email: office@janus.com
          </p>
        </section>
        
      </div>
    </Card>

  </Page>
);

export default PrivacyPolicy;
